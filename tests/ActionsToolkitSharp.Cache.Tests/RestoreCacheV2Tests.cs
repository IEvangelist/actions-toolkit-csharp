// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// V2 RestoreCache orchestration tests — exercise <see cref="DefaultCacheClient"/>
/// against fake Twirp + signed-URL transports. Mirrors upstream
/// <c>__tests__/restoreCacheV2.test.ts</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class RestoreCacheV2Tests
{
    [Fact(DisplayName = "restoreCache throws CacheValidationException when paths is empty")]
    public async Task RestoreCache_Throws_OnEmptyPaths()
    {
        using var env = SeedEnv();
        var (client, _, _) = BuildClient();

        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.RestoreCacheAsync([], "key"));
    }

    [Fact(DisplayName = "restoreCache throws CacheValidationException when too many keys")]
    public async Task RestoreCache_Throws_OnTooManyKeys()
    {
        using var env = SeedEnv();
        var (client, _, _) = BuildClient();

        var manyRestoreKeys = Enumerable.Range(0, 10).Select(i => $"k-{i}").ToArray();
        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.RestoreCacheAsync(["bin"], "primary", manyRestoreKeys));
    }

    [Fact(DisplayName = "restoreCache throws CacheValidationException when a key contains commas")]
    public async Task RestoreCache_Throws_OnCommaInKey()
    {
        using var env = SeedEnv();
        var (client, _, _) = BuildClient();

        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.RestoreCacheAsync(["bin"], "primary", ["bad,key"]));
    }

    [Fact(DisplayName = "restoreCache returns null when GetCacheEntryDownloadURL returns ok=false")]
    public async Task RestoreCache_ReturnsNull_OnMiss()
    {
        using var env = SeedEnv();
        var (client, twirp, _) = BuildClient();
        twirp.GetResponder = _ => new GetCacheEntryDownloadUrlResponse { Ok = false };

        var matched = await client.RestoreCacheAsync(["bin"], "build-x");

        Assert.Null(matched);
    }

    [Fact(DisplayName = "restoreCache with lookupOnly returns matched key without downloading")]
    public async Task RestoreCache_LookupOnly_SkipsDownload()
    {
        using var env = SeedEnv();
        var (client, twirp, factory) = BuildClient();
        twirp.GetResponder = _ => new GetCacheEntryDownloadUrlResponse
        {
            Ok = true,
            SignedDownloadUrl = "https://blob/dl",
            MatchedKey = "build-x",
        };

        var matched = await client.RestoreCacheAsync(
            ["bin"], "build-x", options: new RestoreCacheOptions { LookupOnly = true });

        Assert.Equal("build-x", matched);
        Assert.Empty(factory.Handler.Requests); // no GET to the signed URL
    }

    [Fact(DisplayName = "restoreCache downloads and extracts archive on hit")]
    public async Task RestoreCache_Downloads_OnHit()
    {
        using var env = SeedEnv();

        // Build a real tar+zstd archive so the extract step has real data to work with.
        using var src = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(src.Path, "marker.txt"), "from-cache");
        var archivePath = Path.Combine(src.Path, "cache.tzst");
        await CacheTar.CreateAsync(archivePath, src.Path, ["marker.txt"]);
        var archiveBytes = await File.ReadAllBytesAsync(archivePath);

        var twirp = new FakeCacheTwirpService
        {
            GetResponder = _ => new GetCacheEntryDownloadUrlResponse
            {
                Ok = true,
                SignedDownloadUrl = "https://blob/dl",
                MatchedKey = "build-x",
            },
        };
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(archiveBytes),
            });
        using var factory = new FakeHttpClientFactory(handler);
        var client = new DefaultCacheClient(twirp, factory);

        using var dest = new TempWorkspace();
        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", dest.Path);
        try
        {
            var matched = await client.RestoreCacheAsync(["marker.txt"], "build-x");
            Assert.Equal("build-x", matched);
            Assert.Equal("from-cache",
                await File.ReadAllTextAsync(Path.Combine(dest.Path, "marker.txt")));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }

    [Fact(DisplayName = "restoreCache passes restoreKeys through to GetCacheEntryDownloadURL")]
    public async Task RestoreCache_PassesRestoreKeys()
    {
        using var env = SeedEnv();
        var (client, twirp, _) = BuildClient();

        twirp.GetResponder = _ => new GetCacheEntryDownloadUrlResponse { Ok = false };

        await client.RestoreCacheAsync(["bin"], "primary", ["fallback-1", "fallback-2"]);

        var request = Assert.Single(twirp.GetCalls);
        Assert.Equal("primary", request.Key);
        Assert.NotNull(request.RestoreKeys);
        Assert.Equal(["fallback-1", "fallback-2"], request.RestoreKeys!);
    }

    [Fact(DisplayName = "restoreCache throws CacheServiceUnavailableException when ACTIONS_RESULTS_URL is missing")]
    public async Task RestoreCache_Throws_WhenServiceUnavailable()
    {
        using var env = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", null)
            .Set("ACTIONS_CACHE_URL", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");
        var (client, _, _) = BuildClient();

        await Assert.ThrowsAsync<CacheServiceUnavailableException>(async () =>
            await client.RestoreCacheAsync(["bin"], "key"));
    }

    private static EnvironmentScopeBag SeedEnv() =>
        new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_RUNTIME_TOKEN", "ghs_test")
            .Set("ACTIONS_CACHE_URL", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");

    private static (DefaultCacheClient client, FakeCacheTwirpService twirp, FakeHttpClientFactory factory) BuildClient()
    {
        var twirp = new FakeCacheTwirpService();
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.OK));
        var factory = new FakeHttpClientFactory(handler);
        return (new DefaultCacheClient(twirp, factory), twirp, factory);
    }
}
