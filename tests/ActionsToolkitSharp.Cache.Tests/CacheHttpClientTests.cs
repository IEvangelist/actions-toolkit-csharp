// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Tests for the signed-URL HTTP transport used between the Twirp Create /
/// Finalize calls. Mirrors upstream <c>__tests__/cacheHttpClient.test.ts</c>
/// but is scoped to the orchestration shape since we don't ship a separate
/// public <c>cacheHttpClient</c> seam.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class CacheHttpClientTests
{
    [Fact(DisplayName = "blob PUT upload sets x-ms-blob-type=BlockBlob")]
    public async Task BlobUpload_SetsBlockBlobHeader()
    {
        using var env = SeedEnv();
        var twirp = new FakeCacheTwirpService();
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var factory = new FakeHttpClientFactory(handler);
        var client = new DefaultCacheClient(twirp, factory);

        using var workspace = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "f.txt"), "x");
        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace.Path);
        try
        {
            await client.SaveCacheAsync(["f.txt"], "build-x");
            var put = Assert.Single(handler.Requests, r => r.Method == HttpMethod.Put);
            Assert.Equal("BlockBlob", put.Headers.GetValues("x-ms-blob-type").Single());
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }

    [Fact(DisplayName = "blob PUT propagates upstream non-2xx as CacheServiceException")]
    public async Task BlobUpload_NonSuccess_Throws()
    {
        using var env = SeedEnv();
        var twirp = new FakeCacheTwirpService();
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Forbidden));
        using var factory = new FakeHttpClientFactory(handler);
        var client = new DefaultCacheClient(twirp, factory);

        using var workspace = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "f.txt"), "x");
        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace.Path);
        try
        {
            await Assert.ThrowsAsync<CacheServiceException>(async () =>
                await client.SaveCacheAsync(["f.txt"], "build-x"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }

    [Fact(DisplayName = "blob GET propagates upstream non-2xx as CacheServiceException")]
    public async Task BlobDownload_NonSuccess_Throws()
    {
        using var env = SeedEnv();
        var twirp = new FakeCacheTwirpService
        {
            GetResponder = _ => new GetCacheEntryDownloadUrlResponse
            {
                Ok = true,
                SignedDownloadUrl = "https://blob/dl",
                MatchedKey = "build-x",
            },
        };
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound));
        using var factory = new FakeHttpClientFactory(handler);
        var client = new DefaultCacheClient(twirp, factory);

        using var dest = new TempWorkspace();
        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", dest.Path);
        try
        {
            await Assert.ThrowsAsync<CacheServiceException>(async () =>
                await client.RestoreCacheAsync(["bin"], "build-x"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }

    private static EnvironmentScopeBag SeedEnv() =>
        new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_RUNTIME_TOKEN", "ghs_test")
            .Set("ACTIONS_CACHE_URL", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");
}
