// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Top-level <see cref="ICacheClient.SaveCacheAsync(System.Collections.Generic.IReadOnlyList{string}, string, SaveCacheOptions?, bool, System.Threading.CancellationToken)"/>
/// dispatcher tests — the ones upstream
/// <c>__tests__/saveCache.test.ts</c> exercises before delegating to the V1
/// or V2 implementation. With our V2-only port these tests assert that key
/// and path validation happens before any transport calls.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class SaveCacheTests
{
    [Fact(DisplayName = "saveCache validates paths before any transport call")]
    public async Task ValidatesPaths_BeforeTransport()
    {
        using var env = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_RUNTIME_TOKEN", "ghs_test")
            .Set("GITHUB_SERVER_URL", "https://github.com");
        var twirp = new FakeCacheTwirpService();
        using var factory = new FakeHttpClientFactory(new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = new DefaultCacheClient(twirp, factory);

        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.SaveCacheAsync([], "key"));
        Assert.Empty(twirp.CreateCalls);
    }

    [Fact(DisplayName = "saveCache validates key before any transport call")]
    public async Task ValidatesKey_BeforeTransport()
    {
        using var env = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_RUNTIME_TOKEN", "ghs_test")
            .Set("GITHUB_SERVER_URL", "https://github.com");
        var twirp = new FakeCacheTwirpService();
        using var factory = new FakeHttpClientFactory(new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = new DefaultCacheClient(twirp, factory);

        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.SaveCacheAsync(["bin"], "bad,key"));
        Assert.Empty(twirp.CreateCalls);
    }

    [Fact(DisplayName = "saveCache returns CacheEntry on success with the requested key")]
    public async Task SaveCache_ReturnsEntry()
    {
        using var env = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_RUNTIME_TOKEN", "ghs_test")
            .Set("GITHUB_SERVER_URL", "https://github.com");

        using var workspace = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "f.txt"), "hi");
        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace.Path);
        try
        {
            var twirp = new FakeCacheTwirpService();
            var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
            using var factory = new FakeHttpClientFactory(handler);
            var client = new DefaultCacheClient(twirp, factory);

            var entry = await client.SaveCacheAsync(["f.txt"], "build-x");
            Assert.NotNull(entry);
            Assert.Equal("build-x", entry!.Key);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }
}
