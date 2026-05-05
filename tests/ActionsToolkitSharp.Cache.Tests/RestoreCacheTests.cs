// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Top-level <see cref="ICacheClient.RestoreCacheAsync"/> dispatcher tests
/// — the ones upstream <c>__tests__/restoreCache.test.ts</c> exercises
/// before delegating to V1 or V2 implementations. With our V2-only port
/// these tests assert that validation runs before any transport call.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class RestoreCacheTests
{
    [Fact(DisplayName = "restoreCache validates paths before any transport call")]
    public async Task ValidatesPaths_BeforeTransport()
    {
        using var env = SeedEnv();
        var twirp = new FakeCacheTwirpService();
        using var factory = new FakeHttpClientFactory(new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = new DefaultCacheClient(twirp, factory);

        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.RestoreCacheAsync([], "key"));
        Assert.Empty(twirp.GetCalls);
    }

    [Fact(DisplayName = "restoreCache enforces the 10-key cap (primary + restore keys)")]
    public async Task EnforcesKeyCap()
    {
        using var env = SeedEnv();
        var twirp = new FakeCacheTwirpService();
        using var factory = new FakeHttpClientFactory(new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = new DefaultCacheClient(twirp, factory);

        var manyRestoreKeys = Enumerable.Range(0, 10).Select(i => $"k-{i}").ToArray();
        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.RestoreCacheAsync(["bin"], "primary", manyRestoreKeys));
        Assert.Empty(twirp.GetCalls);
    }

    [Fact(DisplayName = "restoreCache returns null on cache miss without throwing")]
    public async Task ReturnsNull_OnMiss()
    {
        using var env = SeedEnv();
        var twirp = new FakeCacheTwirpService();
        using var factory = new FakeHttpClientFactory(new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = new DefaultCacheClient(twirp, factory);

        var matched = await client.RestoreCacheAsync(["bin"], "build-x");
        Assert.Null(matched);
    }

    private static EnvironmentScopeBag SeedEnv() =>
        new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_RUNTIME_TOKEN", "ghs_test")
            .Set("ACTIONS_CACHE_URL", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");
}
