// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Tests for download-shape options and the blob-GET transport. Mirrors
/// upstream <c>__tests__/downloadUtils.test.ts</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class DownloadUtilsTests
{
    [Fact(DisplayName = "RestoreCacheOptions defaults are null (provider supplies defaults)")]
    public void RestoreCacheOptions_Defaults_AreNull()
    {
        var options = new RestoreCacheOptions();
        Assert.False(options.LookupOnly);
        Assert.Null(options.DownloadConcurrency);
        Assert.Null(options.TimeoutInMs);
        Assert.Null(options.SegmentTimeoutInMs);
    }

    [Fact(DisplayName = "RestoreCacheOptions round-trips explicit values")]
    public void RestoreCacheOptions_RoundTrips()
    {
        var options = new RestoreCacheOptions
        {
            LookupOnly = true,
            DownloadConcurrency = 8,
            TimeoutInMs = 30_000,
            SegmentTimeoutInMs = 600_000,
        };
        Assert.True(options.LookupOnly);
        Assert.Equal(8, options.DownloadConcurrency);
        Assert.Equal(30_000, options.TimeoutInMs);
        Assert.Equal(600_000, options.SegmentTimeoutInMs);
    }

    [Fact(DisplayName = "blob GET writes the archive bytes to disk before extracting")]
    public async Task BlobGet_WritesArchive()
    {
        using var env = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_RUNTIME_TOKEN", "ghs_test")
            .Set("GITHUB_SERVER_URL", "https://github.com");

        // Build a real archive
        using var src = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(src.Path, "p.txt"), "payload");
        var archivePath = Path.Combine(src.Path, "cache.tzst");
        await CacheTar.CreateAsync(archivePath, src.Path, ["p.txt"]);
        var bytes = await File.ReadAllBytesAsync(archivePath);

        var twirp = new FakeCacheTwirpService
        {
            GetResponder = _ => new GetCacheEntryDownloadUrlResponse
            {
                Ok = true,
                SignedDownloadUrl = "https://blob/dl",
                MatchedKey = "build-x",
            },
        };
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes),
        });
        using var factory = new FakeHttpClientFactory(handler);
        var client = new DefaultCacheClient(twirp, factory);

        using var dest = new TempWorkspace();
        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", dest.Path);
        try
        {
            var matched = await client.RestoreCacheAsync(["p.txt"], "build-x");
            Assert.Equal("build-x", matched);
            Assert.Equal("payload", await File.ReadAllTextAsync(Path.Combine(dest.Path, "p.txt")));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }
}
