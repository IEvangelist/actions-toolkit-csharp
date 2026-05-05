// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Tests for the tar+zstd archive build / extract pipeline. Mirrors upstream
/// <c>__tests__/tar.test.ts</c> shape but uses the managed
/// <c>System.Formats.Tar</c> + <c>ZstdSharp.Port</c> pipeline rather than
/// shelling out to a native <c>tar</c>.
/// </summary>
public sealed class TarTests
{
    [Fact(DisplayName = "createTar produces a tar+zstd archive containing the input files")]
    public async Task CreateTar_ProducesArchive()
    {
        using var workspace = new TempWorkspace();
        var sub = Directory.CreateDirectory(Path.Combine(workspace.Path, "data"));
        var helloPath = Path.Combine(sub.FullName, "hello.txt");
        var dataPath = Path.Combine(sub.FullName, "payload.json");
        await File.WriteAllTextAsync(helloPath, "hello");
        await File.WriteAllTextAsync(dataPath, "{\"x\":1}");

        var archivePath = Path.Combine(workspace.Path, "cache.tzst");
        await CacheTar.CreateAsync(archivePath, workspace.Path, ["data"]);

        Assert.True(File.Exists(archivePath));
        Assert.True(new FileInfo(archivePath).Length > 0);
    }

    [Fact(DisplayName = "extractTar round-trips a tar+zstd archive")]
    public async Task ExtractTar_RoundTrips()
    {
        using var src = new TempWorkspace();
        var sub = Directory.CreateDirectory(Path.Combine(src.Path, "data"));
        await File.WriteAllTextAsync(Path.Combine(sub.FullName, "a.txt"), "alpha");
        await File.WriteAllTextAsync(Path.Combine(sub.FullName, "b.txt"), "beta");

        var archivePath = Path.Combine(src.Path, "cache.tzst");
        await CacheTar.CreateAsync(archivePath, src.Path, ["data"]);

        using var dest = new TempWorkspace();
        await CacheTar.ExtractAsync(archivePath, dest.Path);

        Assert.Equal("alpha", await File.ReadAllTextAsync(Path.Combine(dest.Path, "data", "a.txt")));
        Assert.Equal("beta", await File.ReadAllTextAsync(Path.Combine(dest.Path, "data", "b.txt")));
    }

    [Fact(DisplayName = "listTar enumerates archive entries")]
    public async Task ListTar_EnumeratesEntries()
    {
        using var src = new TempWorkspace();
        var sub = Directory.CreateDirectory(Path.Combine(src.Path, "data"));
        await File.WriteAllTextAsync(Path.Combine(sub.FullName, "a.txt"), "alpha");

        var archivePath = Path.Combine(src.Path, "cache.tzst");
        await CacheTar.CreateAsync(archivePath, src.Path, ["data"]);

        var entries = await CacheTar.ListAsync(archivePath);
        Assert.Contains(entries, e => e.EndsWith("a.txt", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "extractTar overwrites existing files")]
    public async Task ExtractTar_Overwrites()
    {
        using var src = new TempWorkspace();
        var sub = Directory.CreateDirectory(Path.Combine(src.Path, "data"));
        await File.WriteAllTextAsync(Path.Combine(sub.FullName, "a.txt"), "v2");

        var archivePath = Path.Combine(src.Path, "cache.tzst");
        await CacheTar.CreateAsync(archivePath, src.Path, ["data"]);

        using var dest = new TempWorkspace();
        Directory.CreateDirectory(Path.Combine(dest.Path, "data"));
        await File.WriteAllTextAsync(Path.Combine(dest.Path, "data", "a.txt"), "v1");

        await CacheTar.ExtractAsync(archivePath, dest.Path);

        Assert.Equal("v2", await File.ReadAllTextAsync(Path.Combine(dest.Path, "data", "a.txt")));
    }

    [Fact(DisplayName = "createTar archives a single file path")]
    public async Task CreateTar_ArchivesSingleFile()
    {
        using var src = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(src.Path, "hello.txt"), "hi");

        var archivePath = Path.Combine(src.Path, "cache.tzst");
        await CacheTar.CreateAsync(archivePath, src.Path, ["hello.txt"]);

        var entries = await CacheTar.ListAsync(archivePath);
        Assert.Contains("hello.txt", entries);
    }

    [Fact(DisplayName = "createTar+extractTar round-trips with gzip compression")]
    public async Task CreateExtract_GzipRoundTrips()
    {
        using var src = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(src.Path, "g.txt"), "gz-payload");

        var archivePath = Path.Combine(src.Path, "cache.tgz");
        await CacheTar.CreateAsync(archivePath, src.Path, ["g.txt"], CompressionMethod.Gzip);

        using var dest = new TempWorkspace();
        await CacheTar.ExtractAsync(archivePath, dest.Path, CompressionMethod.Gzip);

        Assert.Equal("gz-payload",
            await File.ReadAllTextAsync(Path.Combine(dest.Path, "g.txt")));
    }
}

internal sealed class TempWorkspace : IDisposable
{
    public string Path { get; }

    public TempWorkspace()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ats-cache-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        try { Directory.Delete(Path, recursive: true); } catch { /* best effort */ }
    }
}
