// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/upload-zip-specification.test.ts</c> and
/// <c>__tests__/zip.test.ts</c>: validates the produced zip stream is
/// well-formed and that the compression-level mapping is correct.
/// </summary>
public sealed class ZipUploadStreamTests : IDisposable
{
    private readonly string _root;

    public ZipUploadStreamTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "actions-toolkit-zip-stream-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
        }
    }

    [Theory(DisplayName = "MapCompressionLevel maps zlib levels onto .NET levels")]
    [InlineData(null, CompressionLevel.Optimal)]
    [InlineData(0, CompressionLevel.NoCompression)]
    [InlineData(1, CompressionLevel.Fastest)]
    [InlineData(3, CompressionLevel.Fastest)]
    [InlineData(4, CompressionLevel.Optimal)]
    [InlineData(6, CompressionLevel.Optimal)]
    [InlineData(7, CompressionLevel.SmallestSize)]
    [InlineData(9, CompressionLevel.SmallestSize)]
    public void MapsCompressionLevels(int? level, CompressionLevel expected) =>
        Assert.Equal(expected, ZipUploadStream.MapCompressionLevel(level));

    [Fact(DisplayName = "CreateAsync produces a readable zip with the expected entries")]
    public async Task ProducesReadableZip()
    {
        var fileA = Path.Combine(_root, "hello.txt");
        var nested = Path.Combine(_root, "nested");
        Directory.CreateDirectory(nested);
        var fileB = Path.Combine(nested, "world.txt");
        await File.WriteAllTextAsync(fileA, "hello");
        await File.WriteAllTextAsync(fileB, "world");

        var spec = UploadZipSpecification.GetUploadZipSpecification([fileA, fileB], _root);
        var result = await ZipUploadStream.CreateAsync(spec, compressionLevel: 6, CancellationToken.None);

        try
        {
            Assert.Equal(result.Content.Length, result.UploadSize);

            using var archive = new ZipArchive(result.Content, ZipArchiveMode.Read, leaveOpen: true);
            Assert.Collection(
                archive.Entries.OrderBy(e => e.FullName, StringComparer.Ordinal),
                entry => Assert.Equal("hello.txt", entry.FullName),
                entry => Assert.Equal("nested/world.txt", entry.FullName));
        }
        finally
        {
            await result.Content.DisposeAsync();
        }
    }

    [Fact(DisplayName = "CreateAsync emits a directory entry terminated with /")]
    public async Task EmitsDirectoryEntry()
    {
        var emptyDir = Path.Combine(_root, "empty");
        Directory.CreateDirectory(emptyDir);

        var spec = UploadZipSpecification.GetUploadZipSpecification([emptyDir], _root);
        var result = await ZipUploadStream.CreateAsync(spec, compressionLevel: null, CancellationToken.None);

        try
        {
            using var archive = new ZipArchive(result.Content, ZipArchiveMode.Read, leaveOpen: true);
            var entry = Assert.Single(archive.Entries);
            Assert.Equal("empty/", entry.FullName);
        }
        finally
        {
            await result.Content.DisposeAsync();
        }
    }
}
