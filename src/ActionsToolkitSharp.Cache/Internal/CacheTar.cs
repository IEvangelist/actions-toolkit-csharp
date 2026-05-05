// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Formats.Tar;
using ZstdSharp;

namespace ActionsToolkitSharp.Cache.Internal;

/// <summary>
/// AOT-clean tar+zstd archive create / extract. Mirrors the upstream
/// <c>@actions/cache/src/internal/tar.ts</c> tar pipeline but uses
/// <see cref="System.Formats.Tar"/> for the tar layer and
/// <c>ZstdSharp.Port</c> for the zstd layer instead of shelling out to a
/// native <c>tar</c> + <c>zstd</c> binary.
/// </summary>
internal static class CacheTar
{
    /// <summary>
    /// Builds a tar+zstd archive at <paramref name="archivePath"/> from the
    /// given <paramref name="paths"/> rooted at <paramref name="workspace"/>.
    /// Each path is included with its relative path as the entry name. When
    /// <paramref name="enableCrossOsArchive"/> is true, paths are normalized
    /// to forward slashes for cross-platform compatibility.
    /// </summary>
    public static async ValueTask CreateAsync(
        string archivePath,
        string workspace,
        IReadOnlyList<string> paths,
        CompressionMethod method = CompressionMethod.ZstdWithoutLong,
        bool enableCrossOsArchive = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(archivePath);
        ArgumentException.ThrowIfNullOrEmpty(workspace);
        ArgumentNullException.ThrowIfNull(paths);

        var directory = Path.GetDirectoryName(archivePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var output = File.Create(archivePath);
        await using var compressor = OpenWriteCompressor(output, method);
        await using var tar = new TarWriter(compressor, TarEntryFormat.Pax, leaveOpen: true);

        foreach (var pattern in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddPathAsync(tar, workspace, pattern, enableCrossOsArchive, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Extracts a tar+zstd archive at <paramref name="archivePath"/> into
    /// <paramref name="workspace"/> (creating it if missing). Mirrors the
    /// upstream <c>extractTar</c> behavior of overwriting existing files.
    /// </summary>
    public static async ValueTask ExtractAsync(
        string archivePath,
        string workspace,
        CompressionMethod method = CompressionMethod.ZstdWithoutLong,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(archivePath);
        ArgumentException.ThrowIfNullOrEmpty(workspace);

        Directory.CreateDirectory(workspace);

        await using var input = File.OpenRead(archivePath);
        await using var decompressor = OpenReadDecompressor(input, method);
        await TarFile.ExtractToDirectoryAsync(
            decompressor,
            workspace,
            overwriteFiles: true,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the names of every entry in a tar+zstd archive — used by the
    /// upstream <c>listTar</c> debug helper. Useful for the unit tests.
    /// </summary>
    public static async ValueTask<IReadOnlyList<string>> ListAsync(
        string archivePath,
        CompressionMethod method = CompressionMethod.ZstdWithoutLong,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(archivePath);

        await using var input = File.OpenRead(archivePath);
        await using var decompressor = OpenReadDecompressor(input, method);
        await using var reader = new TarReader(decompressor, leaveOpen: true);

        var entries = new List<string>();
        TarEntry? entry;
        while ((entry = await reader.GetNextEntryAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false)) is not null)
        {
            entries.Add(entry.Name);
        }

        return entries;
    }

    private static Stream OpenWriteCompressor(Stream destination, CompressionMethod method)
    {
        return method switch
        {
            CompressionMethod.Gzip => new System.IO.Compression.GZipStream(
                destination,
                System.IO.Compression.CompressionMode.Compress,
                leaveOpen: true),
            _ => new CompressionStream(destination, level: 3, leaveOpen: true),
        };
    }

    private static Stream OpenReadDecompressor(Stream source, CompressionMethod method)
    {
        return method switch
        {
            CompressionMethod.Gzip => new System.IO.Compression.GZipStream(
                source,
                System.IO.Compression.CompressionMode.Decompress,
                leaveOpen: true),
            _ => new DecompressionStream(source, leaveOpen: true),
        };
    }

    private static async ValueTask AddPathAsync(
        TarWriter tar,
        string workspace,
        string pattern,
        bool enableCrossOsArchive,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.IsPathRooted(pattern)
            ? pattern
            : Path.GetFullPath(Path.Combine(workspace, pattern));

        if (Directory.Exists(fullPath))
        {
            await AddDirectoryAsync(tar, workspace, fullPath, enableCrossOsArchive, cancellationToken)
                .ConfigureAwait(false);
        }
        else if (File.Exists(fullPath))
        {
            await AddFileAsync(tar, workspace, fullPath, enableCrossOsArchive, cancellationToken)
                .ConfigureAwait(false);
        }
        // else: silently skip — upstream resolvePaths() filters non-existent globs.
    }

    private static async ValueTask AddDirectoryAsync(
        TarWriter tar,
        string workspace,
        string fullPath,
        bool enableCrossOsArchive,
        CancellationToken cancellationToken)
    {
        var dirRel = NormalizeEntryName(workspace, fullPath, enableCrossOsArchive);
        await tar.WriteEntryAsync(fullPath, dirRel.TrimEnd('/') + "/", cancellationToken)
            .ConfigureAwait(false);

        foreach (var file in Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddFileAsync(tar, workspace, file, enableCrossOsArchive, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static async ValueTask AddFileAsync(
        TarWriter tar,
        string workspace,
        string fullPath,
        bool enableCrossOsArchive,
        CancellationToken cancellationToken)
    {
        var entryName = NormalizeEntryName(workspace, fullPath, enableCrossOsArchive);
        await tar.WriteEntryAsync(fullPath, entryName, cancellationToken).ConfigureAwait(false);
    }

    private static string NormalizeEntryName(string workspace, string fullPath, bool enableCrossOsArchive)
    {
        var rel = Path.GetRelativePath(workspace, fullPath);
        if (rel == "." || string.IsNullOrEmpty(rel))
        {
            rel = Path.GetFileName(fullPath);
        }

        // Tar entries are POSIX path-separated. The cross-os-archive flag is
        // accepted but always honored (forward slashes are required by the
        // POSIX tar spec regardless of source OS).
        _ = enableCrossOsArchive;
        return rel.Replace('\\', '/');
    }
}
