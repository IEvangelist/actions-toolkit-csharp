// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Tests;

/// <summary>
/// Mirrors the upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/__tests__/tool-cache.test.ts">
/// <c>tool-cache.test.ts</c></a> suite for the cacheDir/cacheFile/find APIs.
/// </summary>
public sealed class ToolCacheTests : IDisposable
{
    private readonly TempCacheFixture _fixture = new();

    public void Dispose() => _fixture.Dispose();

    private static IToolCacheService NewService(System.Net.Http.HttpClient? http = null) =>
        new DefaultToolCacheService(
            httpClient: new StubHttpClient(http ?? new System.Net.Http.HttpClient()),
            retryHelper: new DefaultRetryHelper(maxAttempts: 1, minSeconds: 0, maxSeconds: 0,
                sleep: _ => Task.CompletedTask, info: _ => { }));

    private static string CreateSourceFolder(string root, params string[] files)
    {
        var src = Path.Combine(root, "src-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(src);
        foreach (var f in files)
        {
            File.WriteAllText(Path.Combine(src, f), $"contents of {f}");
        }
        return src;
    }

    [Fact(DisplayName = "downloadTool writes file from successful response")]
    public async Task DownloadToolWritesFileFromSuccessfulResponse()
    {
        var handler = new TestHttpMessageHandler(_ =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("payload")),
            };
            return resp;
        });
        using var http = new System.Net.Http.HttpClient(handler);
        var svc = NewService(http);

        var dest = Path.Combine(_fixture.TempRoot, "out-" + Guid.NewGuid().ToString("N") + ".bin");
        var result = await svc.DownloadToolAsync("https://example.test/file", dest);
        Assert.Equal(dest, result);
        Assert.Equal("payload", File.ReadAllText(dest));
    }

    [Fact(DisplayName = "downloadTool defaults destination to RUNNER_TEMP")]
    public async Task DownloadToolDefaultsDestinationToRunnerTemp()
    {
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) });
        using var http = new System.Net.Http.HttpClient(handler);
        var svc = NewService(http);

        var path = await svc.DownloadToolAsync("https://example.test/file");
        Assert.StartsWith(_fixture.TempRoot, path, StringComparison.Ordinal);
        Assert.True(File.Exists(path));
    }

    [Fact(DisplayName = "downloadTool sends Authorization header when provided")]
    public async Task DownloadToolSendsAuthorizationHeader()
    {
        string? auth = null;
        var handler = new TestHttpMessageHandler(req =>
        {
            auth = req.Headers.Authorization?.ToString()
                ?? (req.Headers.TryGetValues("Authorization", out var v) ? string.Join(",", v) : null);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([0]) };
        });
        using var http = new System.Net.Http.HttpClient(handler);
        var svc = NewService(http);

        await svc.DownloadToolAsync("https://example.test/file", auth: "Bearer token123");
        Assert.Contains("Bearer token123", auth);
    }

    [Fact(DisplayName = "downloadTool sends extra headers when provided")]
    public async Task DownloadToolSendsExtraHeaders()
    {
        string? customHeader = null;
        var handler = new TestHttpMessageHandler(req =>
        {
            if (req.Headers.TryGetValues("X-Custom", out var v))
            {
                customHeader = string.Join(",", v);
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([0]) };
        });
        using var http = new System.Net.Http.HttpClient(handler);
        var svc = NewService(http);

        await svc.DownloadToolAsync(
            "https://example.test/file",
            headers: new Dictionary<string, string> { ["X-Custom"] = "yes" });

        Assert.Equal("yes", customHeader);
    }

    [Fact(DisplayName = "downloadTool throws HttpError on non-2xx")]
    public async Task DownloadToolThrowsHttpErrorOnNon2xx()
    {
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("nope") });
        using var http = new System.Net.Http.HttpClient(handler);
        var svc = NewService(http);

        var ex = await Assert.ThrowsAsync<HttpError>(() =>
            svc.DownloadToolAsync("https://example.test/file").AsTask());
        Assert.Equal(HttpStatusCode.NotFound, ex.HttpStatusCode);
    }

    [Fact(DisplayName = "downloadTool throws when destination already exists")]
    public async Task DownloadToolThrowsWhenDestinationAlreadyExists()
    {
        var dest = Path.Combine(_fixture.TempRoot, Guid.NewGuid().ToString("N"));
        File.WriteAllText(dest, "existing");

        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([0]) });
        using var http = new System.Net.Http.HttpClient(handler);
        var svc = NewService(http);

        await Assert.ThrowsAsync<IOException>(() =>
            svc.DownloadToolAsync("https://example.test/file", dest).AsTask());
    }

    [Fact(DisplayName = "downloadTool retries on 5xx")]
    public async Task DownloadToolRetriesOn5xx()
    {
        var attempts = 0;
        var handler = new TestHttpMessageHandler(_ =>
        {
            attempts++;
            return attempts < 3
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2]) };
        });
        using var http = new System.Net.Http.HttpClient(handler);

        var svc = new DefaultToolCacheService(
            new StubHttpClient(http),
            new DefaultRetryHelper(
                maxAttempts: 3, minSeconds: 0, maxSeconds: 0,
                sleep: _ => Task.CompletedTask, info: _ => { }));

        var dest = Path.Combine(_fixture.TempRoot, Guid.NewGuid().ToString("N"));
        await svc.DownloadToolAsync("https://example.test/file", dest);
        Assert.True(File.Exists(dest));
        Assert.Equal(3, attempts);
    }

    [Fact(DisplayName = "downloadTool does not retry on 404")]
    public async Task DownloadToolDoesNotRetryOn404()
    {
        var attempts = 0;
        var handler = new TestHttpMessageHandler(_ =>
        {
            attempts++;
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        using var http = new System.Net.Http.HttpClient(handler);

        var svc = new DefaultToolCacheService(
            new StubHttpClient(http),
            new DefaultRetryHelper(
                maxAttempts: 3, minSeconds: 0, maxSeconds: 0,
                sleep: _ => Task.CompletedTask, info: _ => { }));

        await Assert.ThrowsAsync<HttpError>(() =>
            svc.DownloadToolAsync("https://example.test/file").AsTask());
        Assert.Equal(1, attempts);
    }

    [Fact(DisplayName = "cacheDir copies a directory into the cache")]
    public async Task CacheDirCopiesADirectoryIntoTheCache()
    {
        var svc = NewService();
        var src = CreateSourceFolder(_fixture.TempRoot, "node", "node.exe");

        var dest = await svc.CacheDirAsync(src, "node", "16.0.0", "x64");
        Assert.True(Directory.Exists(dest));
        Assert.True(File.Exists(Path.Combine(dest, "node")));
        Assert.True(File.Exists(Path.Combine(dest, "node.exe")));
        Assert.True(File.Exists(dest + ".complete"));
    }

    [Fact(DisplayName = "cacheDir cleans the version it finds")]
    public async Task CacheDirCleansVersion()
    {
        var svc = NewService();
        var src = CreateSourceFolder(_fixture.TempRoot, "node");

        var dest = await svc.CacheDirAsync(src, "node", "v1.2.3", "x64");
        Assert.EndsWith(Path.Combine("node", "1.2.3", "x64"), dest, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "cacheDir throws when source does not exist")]
    public async Task CacheDirThrowsWhenSourceMissing()
    {
        var svc = NewService();
        var bogus = Path.Combine(_fixture.TempRoot, "nope-" + Guid.NewGuid().ToString("N"));
        await Assert.ThrowsAsync<IOException>(() =>
            svc.CacheDirAsync(bogus, "tool", "1.0.0", "x64").AsTask());
    }

    [Fact(DisplayName = "cacheFile copies a single file into the cache")]
    public async Task CacheFileCopiesSingleFile()
    {
        var svc = NewService();
        var srcFile = Path.Combine(_fixture.TempRoot, "binary.bin");
        File.WriteAllText(srcFile, "data");

        var dest = await svc.CacheFileAsync(srcFile, "renamed.bin", "tool", "2.0.0", "x64");
        Assert.True(File.Exists(Path.Combine(dest, "renamed.bin")));
        Assert.True(File.Exists(dest + ".complete"));
    }

    [Fact(DisplayName = "cacheFile throws when source does not exist")]
    public async Task CacheFileThrowsWhenSourceMissing()
    {
        var svc = NewService();
        var bogus = Path.Combine(_fixture.TempRoot, "missing.bin");
        await Assert.ThrowsAsync<IOException>(() =>
            svc.CacheFileAsync(bogus, "x.bin", "tool", "1.0.0", "x64").AsTask());
    }

    [Fact(DisplayName = "find returns empty when the tool is not cached")]
    public void FindReturnsEmptyWhenNotCached()
    {
        var svc = NewService();
        Assert.Equal(string.Empty, svc.Find("missing-tool", "1.0.0", "x64"));
    }

    [Fact(DisplayName = "find returns the cache path for an exact version")]
    public async Task FindReturnsCachePathForExactVersion()
    {
        var svc = NewService();
        var src = CreateSourceFolder(_fixture.TempRoot, "tool");
        var cached = await svc.CacheDirAsync(src, "thetool", "3.4.5", "x64");

        Assert.Equal(cached, svc.Find("thetool", "3.4.5", "x64"));
    }

    [Fact(DisplayName = "find resolves a version range against the cache")]
    public async Task FindResolvesRange()
    {
        var svc = NewService();
        var src = CreateSourceFolder(_fixture.TempRoot, "tool");
        await svc.CacheDirAsync(src, "ranged", "1.0.0", "x64");
        await svc.CacheDirAsync(src, "ranged", "1.5.0", "x64");
        await svc.CacheDirAsync(src, "ranged", "2.0.0", "x64");

        var match = svc.Find("ranged", "1.x", "x64");
        Assert.EndsWith(Path.Combine("1.5.0", "x64"), match, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "find ignores incomplete cache entries")]
    public async Task FindIgnoresIncompleteEntries()
    {
        var svc = NewService();
        var src = CreateSourceFolder(_fixture.TempRoot, "tool");
        var cached = await svc.CacheDirAsync(src, "incomplete", "1.0.0", "x64");

        File.Delete(cached + ".complete");

        Assert.Equal(string.Empty, svc.Find("incomplete", "1.0.0", "x64"));
    }

    [Fact(DisplayName = "findAllVersions enumerates cached explicit versions")]
    public async Task FindAllVersionsEnumeratesCached()
    {
        var svc = NewService();
        var src = CreateSourceFolder(_fixture.TempRoot, "tool");
        await svc.CacheDirAsync(src, "many", "1.0.0", "x64");
        await svc.CacheDirAsync(src, "many", "2.0.0", "x64");

        var versions = svc.FindAllVersions("many", "x64");
        Assert.Contains("1.0.0", versions);
        Assert.Contains("2.0.0", versions);
    }

    [Fact(DisplayName = "findAllVersions returns empty when nothing cached")]
    public void FindAllVersionsReturnsEmptyWhenNothingCached()
    {
        var svc = NewService();
        var versions = svc.FindAllVersions("never-cached", "x64");
        Assert.Empty(versions);
    }

    [Fact(DisplayName = "extractXar throws PlatformNotSupportedException")]
    public async Task ExtractXarThrows()
    {
        var svc = NewService();
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() =>
            svc.ExtractXarAsync("ignored").AsTask());
    }

    [Fact(DisplayName = "find returns empty for empty version spec")]
    public void FindThrowsForEmptyVersionSpec()
    {
        var svc = NewService();
        Assert.Throws<ArgumentException>(() => svc.Find("tool", string.Empty, "x64"));
    }

    [Fact(DisplayName = "downloadTool throws ArgumentException for empty url")]
    public async Task DownloadToolThrowsForEmptyUrl()
    {
        var svc = NewService();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.DownloadToolAsync(string.Empty).AsTask());
    }
}
