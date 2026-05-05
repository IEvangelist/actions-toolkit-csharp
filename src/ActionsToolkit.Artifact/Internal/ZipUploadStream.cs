// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// AOT-clean replacement for upstream <c>@actions/artifact/src/internal/upload/zip.ts</c>.
/// Streams a zip archive built from an
/// <see cref="UploadZipSpecificationEntry"/> list into memory (or to a
/// temp file when the payload is large enough that buffering would be
/// wasteful) and returns a seekable <see cref="Stream"/> ready to be
/// PUT to the signed upload URL.
/// </summary>
internal static class ZipUploadStream
{
    /// <summary>
    /// Threshold beyond which the staging buffer spills onto disk to avoid
    /// pinning a large MemoryStream in the LOH.
    /// </summary>
    private const long SpillToDiskThreshold = 32L * 1024 * 1024;

    /// <summary>
    /// Builds the zip archive synchronously and returns a stream wrapping
    /// the produced bytes. The stream is positioned at zero and is the
    /// caller's responsibility to dispose.
    /// </summary>
    /// <param name="entries">The (source, destination) entries that drive
    /// archive content.</param>
    /// <param name="compressionLevel">Zlib-style 0–9 compression level (per
    /// upstream); when null upstream's default of 6 is used.</param>
    /// <param name="cancellationToken">Cancels the read of any source file
    /// part-way through.</param>
    public static async Task<ZipUploadResult> CreateAsync(
        IReadOnlyList<UploadZipSpecificationEntry> entries,
        int? compressionLevel,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var totalSourceBytes = SumKnownSourceSizes(entries);
        var resolvedLevel = MapCompressionLevel(compressionLevel);

        Stream backing = totalSourceBytes >= SpillToDiskThreshold
            ? CreateTempFileStream()
            : new MemoryStream();

        try
        {
            using (var archive = new ZipArchive(backing, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await AddEntryAsync(archive, entry, resolvedLevel, cancellationToken).ConfigureAwait(false);
                }
            }

            backing.Position = 0;
            var size = backing.Length;
            return new ZipUploadResult(backing, size);
        }
        catch
        {
            backing.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Maps the upstream Zlib 0–9 ladder onto the four .NET compression
    /// levels exposed by <see cref="System.IO.Compression"/>.
    /// </summary>
    /// <remarks>
    /// .NET does not expose the full 0–9 ladder, so we collapse the
    /// upstream values into the closest equivalent. The default of 6
    /// (when no level is supplied) maps to <see cref="CompressionLevel.Optimal"/>,
    /// matching upstream <c>DEFAULT_COMPRESSION_LEVEL</c>.
    /// </remarks>
    public static CompressionLevel MapCompressionLevel(int? compressionLevel)
    {
        return compressionLevel switch
        {
            null => CompressionLevel.Optimal,
            <= 0 => CompressionLevel.NoCompression,
            >= 1 and <= 3 => CompressionLevel.Fastest,
            >= 4 and <= 6 => CompressionLevel.Optimal,
            _ => CompressionLevel.SmallestSize,
        };
    }

    private static async Task AddEntryAsync(
        ZipArchive archive,
        UploadZipSpecificationEntry entry,
        CompressionLevel level,
        CancellationToken cancellationToken)
    {
        var name = NormalizeZipName(entry.DestinationPath);
        if (entry.SourcePath is null)
        {
            // Empty directory entry: terminate with `/` so unzip tools create the dir.
            if (!name.EndsWith('/'))
            {
                name += "/";
            }

            archive.CreateEntry(name, level);
            return;
        }

        var zipEntry = archive.CreateEntry(name, level);
        await using var sourceStream = new FileStream(
            entry.SourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);
        await using var entryStream = zipEntry.Open();
        await sourceStream.CopyToAsync(entryStream, cancellationToken).ConfigureAwait(false);
    }

    private static string NormalizeZipName(string destinationPath)
    {
        // Convert platform separators to forward slashes (zip-portable) and
        // strip any leading separator so entries are relative to the archive root.
        var name = destinationPath.Replace(Path.DirectorySeparatorChar, '/');
        if (Path.AltDirectorySeparatorChar != '/')
        {
            name = name.Replace(Path.AltDirectorySeparatorChar, '/');
        }

        return name.TrimStart('/');
    }

    private static long SumKnownSourceSizes(IReadOnlyList<UploadZipSpecificationEntry> entries)
    {
        long total = 0;
        foreach (var entry in entries)
        {
            if (entry.SourcePath is null)
            {
                continue;
            }

            try
            {
                total += new FileInfo(entry.SourcePath).Length;
            }
            catch (FileNotFoundException)
            {
                // ignore — the per-entry copy will surface the failure with a clearer message.
            }
            catch (DirectoryNotFoundException)
            {
                // ignore — same reason.
            }
        }

        return total;
    }

    private static FileStream CreateTempFileStream() =>
        new(
            Path.Combine(Path.GetTempPath(), $"actions-toolkit-artifact-zip-{Guid.NewGuid():N}.zip"),
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.DeleteOnClose | FileOptions.Asynchronous);
}

/// <summary>
/// The output of <see cref="ZipUploadStream.CreateAsync"/>. Owns a seekable
/// stream over the produced archive plus the precomputed total size.
/// </summary>
internal sealed record ZipUploadResult(Stream Content, long UploadSize);
