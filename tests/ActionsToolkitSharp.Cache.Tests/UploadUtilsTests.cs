// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Tests for upload-shape options and the blob-PUT transport. Mirrors
/// upstream <c>__tests__/uploadUtils.test.ts</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class UploadUtilsTests
{
    [Fact(DisplayName = "SaveCacheOptions defaults are null (provider supplies defaults)")]
    public void SaveCacheOptions_Defaults_AreNull()
    {
        var options = new SaveCacheOptions();
        Assert.Null(options.UploadConcurrency);
        Assert.Null(options.UploadChunkSize);
        Assert.Null(options.ArchiveSizeBytes);
    }

    [Fact(DisplayName = "SaveCacheOptions round-trips explicit values")]
    public void SaveCacheOptions_RoundTrips()
    {
        var options = new SaveCacheOptions
        {
            UploadConcurrency = 8,
            UploadChunkSize = 64 * 1024 * 1024,
            ArchiveSizeBytes = 12345,
        };
        Assert.Equal(8, options.UploadConcurrency);
        Assert.Equal(64 * 1024 * 1024, options.UploadChunkSize);
        Assert.Equal(12345, options.ArchiveSizeBytes);
    }

    [Fact(DisplayName = "blob PUT body equals the bytes of the archive on disk")]
    public async Task BlobPut_SendsArchiveBytes()
    {
        using var env = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_RUNTIME_TOKEN", "ghs_test")
            .Set("GITHUB_SERVER_URL", "https://github.com");

        byte[]? captured = null;
        var twirp = new FakeCacheTwirpService();
        var handler = new TestHttpMessageHandler((req, ct) =>
        {
            if (req.Method == HttpMethod.Put && req.Content is not null)
            {
                captured = req.Content.ReadAsByteArrayAsync(ct).GetAwaiter().GetResult();
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var factory = new FakeHttpClientFactory(handler);
        var client = new DefaultCacheClient(twirp, factory);

        using var workspace = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "f.txt"), "hello cache");
        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace.Path);
        try
        {
            await client.SaveCacheAsync(["f.txt"], "build-x");
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }

        Assert.NotNull(captured);
        Assert.True(captured!.Length > 0);

        // Round-trip the captured bytes via CacheTar.ExtractAsync.
        var tempArchive = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".tzst");
        await File.WriteAllBytesAsync(tempArchive, captured);
        try
        {
            using var dest = new TempWorkspace();
            await CacheTar.ExtractAsync(tempArchive, dest.Path);
            Assert.Equal("hello cache",
                await File.ReadAllTextAsync(Path.Combine(dest.Path, "f.txt")));
        }
        finally
        {
            File.Delete(tempArchive);
        }
    }
}
