// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using SharpCompress.Common;
using SharpCompress.Writers;

namespace ActionsToolkit.ToolCache.Tests;

/// <summary>
/// Mirrors the upstream extract* tests by building tar/zip/7z fixtures on the
/// fly and asserting that <see cref="IToolCacheService"/> reproduces them.
/// </summary>
public sealed class ExtractionTests : IDisposable
{
    private readonly TempCacheFixture _fixture = new();
    private readonly string _root;

    public ExtractionTests()
    {
        _root = Path.Combine(_fixture.TempRoot, "extraction-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose() => _fixture.Dispose();

    private static IToolCacheService NewService() =>
        new DefaultToolCacheService(
            new StubHttpClient(new System.Net.Http.HttpClient()),
            new DefaultRetryHelper(maxAttempts: 1, minSeconds: 0, maxSeconds: 0,
                sleep: _ => Task.CompletedTask, info: _ => { }));

    private string MakeFile(string name, string contents)
    {
        var path = Path.Combine(_root, name);
        File.WriteAllText(path, contents);
        return path;
    }

    [Fact(DisplayName = "extractZip extracts a zip archive")]
    public async Task ExtractZipExtracts()
    {
        var srcA = MakeFile("a.txt", "alpha");
        var srcB = MakeFile("b.txt", "bravo");
        var zipPath = Path.Combine(_root, "archive.zip");

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(srcA, "a.txt");
            zip.CreateEntryFromFile(srcB, "b.txt");
        }

        var svc = NewService();
        var dest = await svc.ExtractZipAsync(zipPath);
        Assert.True(File.Exists(Path.Combine(dest, "a.txt")));
        Assert.True(File.Exists(Path.Combine(dest, "b.txt")));
        Assert.Equal("alpha", File.ReadAllText(Path.Combine(dest, "a.txt")));
    }

    [Fact(DisplayName = "extractZip extracts to specified destination")]
    public async Task ExtractZipExtractsToDest()
    {
        var src = MakeFile("inner.txt", "hello");
        var zipPath = Path.Combine(_root, "with-dest.zip");
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(src, "inner.txt");
        }

        var svc = NewService();
        var explicitDest = Path.Combine(_root, "explicit-dest");
        var dest = await svc.ExtractZipAsync(zipPath, explicitDest);

        Assert.Equal(explicitDest, dest);
        Assert.True(File.Exists(Path.Combine(dest, "inner.txt")));
    }

    [Fact(DisplayName = "extractTar extracts a tar archive")]
    public async Task ExtractTarExtracts()
    {
        // Build a plain (uncompressed) tar via SharpCompress writer.
        var srcA = MakeFile("a.txt", "alpha");
        var srcB = MakeFile("b.txt", "bravo");
        var tarPath = Path.Combine(_root, "archive.tar");

        using (var fs = File.Create(tarPath))
        using (var writer = WriterFactory.Open(fs, ArchiveType.Tar, new WriterOptions(CompressionType.None)))
        {
            writer.Write("a.txt", srcA);
            writer.Write("b.txt", srcB);
        }

        var svc = NewService();
        var dest = await svc.ExtractTarAsync(tarPath);
        Assert.True(File.Exists(Path.Combine(dest, "a.txt")));
        Assert.True(File.Exists(Path.Combine(dest, "b.txt")));
        Assert.Equal("alpha", File.ReadAllText(Path.Combine(dest, "a.txt")));
    }

    [Fact(DisplayName = "extractTar extracts a gzipped tar archive")]
    public async Task ExtractTarGzExtracts()
    {
        var src = MakeFile("inner.txt", "tar-gz");
        var tarPath = Path.Combine(_root, "archive.tar.gz");

        using (var fs = File.Create(tarPath))
        using (var writer = WriterFactory.Open(fs, ArchiveType.Tar, new WriterOptions(CompressionType.GZip)))
        {
            writer.Write("inner.txt", src);
        }

        var svc = NewService();
        var dest = await svc.ExtractTarAsync(tarPath);
        Assert.True(File.Exists(Path.Combine(dest, "inner.txt")));
        Assert.Equal("tar-gz", File.ReadAllText(Path.Combine(dest, "inner.txt")));
    }

    [Fact(DisplayName = "extractTar accepts informational flags")]
    public async Task ExtractTarAcceptsFlags()
    {
        var src = MakeFile("flags.txt", "flagged");
        var tarPath = Path.Combine(_root, "flags.tar");
        using (var fs = File.Create(tarPath))
        using (var writer = WriterFactory.Open(fs, ArchiveType.Tar, new WriterOptions(CompressionType.None)))
        {
            writer.Write("flags.txt", src);
        }

        var svc = NewService();
        var dest = await svc.ExtractTarAsync(tarPath, flags: ["-x"]);
        Assert.True(File.Exists(Path.Combine(dest, "flags.txt")));
    }

    [Fact(DisplayName = "extractZip throws ArgumentException for empty file path")]
    public async Task ExtractZipThrowsForEmpty()
    {
        var svc = NewService();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.ExtractZipAsync(string.Empty).AsTask());
    }

    [Fact(DisplayName = "extractTar throws ArgumentException for empty file path")]
    public async Task ExtractTarThrowsForEmpty()
    {
        var svc = NewService();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.ExtractTarAsync(string.Empty).AsTask());
    }

    [Fact(DisplayName = "extract7z throws ArgumentException for empty file path")]
    public async Task Extract7zThrowsForEmpty()
    {
        var svc = NewService();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.Extract7zAsync(string.Empty).AsTask());
    }
}
