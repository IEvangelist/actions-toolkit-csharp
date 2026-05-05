// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// V2 SaveCache orchestration tests — exercise <see cref="DefaultCacheClient"/>
/// against fake Twirp + signed-URL transports. Mirrors upstream
/// <c>__tests__/saveCacheV2.test.ts</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class SaveCacheV2Tests
{
    [Fact(DisplayName = "saveCache throws CacheValidationException when paths is empty")]
    public async Task SaveCache_Throws_OnEmptyPaths()
    {
        using var env = SeedEnv();
        var (client, _, _) = BuildClient();

        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.SaveCacheAsync([], "key"));
    }

    [Fact(DisplayName = "saveCache throws CacheValidationException when key contains commas")]
    public async Task SaveCache_Throws_OnCommaInKey()
    {
        using var env = SeedEnv();
        var (client, _, _) = BuildClient();

        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.SaveCacheAsync(["bin"], "a,b"));
    }

    [Fact(DisplayName = "saveCache throws CacheValidationException when key exceeds 512 chars")]
    public async Task SaveCache_Throws_OnLongKey()
    {
        using var env = SeedEnv();
        var (client, _, _) = BuildClient();

        var longKey = new string('a', 513);
        await Assert.ThrowsAsync<CacheValidationException>(async () =>
            await client.SaveCacheAsync(["bin"], longKey));
    }

    [Fact(DisplayName = "saveCache throws CacheServiceUnavailableException when ACTIONS_RESULTS_URL is missing")]
    public async Task SaveCache_Throws_WhenServiceUnavailable()
    {
        using var env = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", null)
            .Set("ACTIONS_CACHE_URL", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");
        var (client, _, _) = BuildClient();

        using var workspace = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "f.txt"), "x");

        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace.Path);
        try
        {
            await Assert.ThrowsAsync<CacheServiceUnavailableException>(async () =>
                await client.SaveCacheAsync(["f.txt"], "key"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }

    [Fact(DisplayName = "saveCache throws ReserveCacheException when CreateCacheEntry returns ok=false")]
    public async Task SaveCache_Throws_OnReserveFailure()
    {
        using var env = SeedEnv();
        var (client, twirp, _) = BuildClient();
        twirp.CreateResponder = _ => new CreateCacheEntryResponse
        {
            Ok = false,
            Message = "already exists",
        };

        using var workspace = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "f.txt"), "x");

        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace.Path);
        try
        {
            await Assert.ThrowsAsync<ReserveCacheException>(async () =>
                await client.SaveCacheAsync(["f.txt"], "build-x"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }

    [Fact(DisplayName = "saveCache throws FinalizeCacheException when finalize returns ok=false")]
    public async Task SaveCache_Throws_OnFinalizeFailure()
    {
        using var env = SeedEnv();
        var (client, twirp, _) = BuildClient();
        twirp.FinalizeResponder = _ => new FinalizeCacheEntryUploadResponse { Ok = false, Message = "fail" };

        using var workspace = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "f.txt"), "x");

        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace.Path);
        try
        {
            await Assert.ThrowsAsync<FinalizeCacheException>(async () =>
                await client.SaveCacheAsync(["f.txt"], "build-x"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }

    [Fact(DisplayName = "saveCache PUTs the archive to the signed URL with x-ms-blob-type")]
    public async Task SaveCache_PutsToSignedUrl()
    {
        using var env = SeedEnv();
        var (client, twirp, factory) = BuildClient();

        using var workspace = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "f.txt"), "hello");

        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace.Path);
        try
        {
            var entry = await client.SaveCacheAsync(["f.txt"], "build-x");

            Assert.NotNull(entry);
            Assert.Equal("build-x", entry!.Key);
            Assert.True(entry.Size > 0);

            var put = Assert.Single(factory.Handler.Requests, r => r.Method == HttpMethod.Put);
            Assert.Equal("https://blob/upload", put.RequestUri!.ToString());
            Assert.True(put.Headers.TryGetValues("x-ms-blob-type", out var blobType));
            Assert.Equal("BlockBlob", blobType!.Single());
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", null);
        }
    }

    [Fact(DisplayName = "saveCache invokes Create, signed PUT, then Finalize in order")]
    public async Task SaveCache_InvokesCreatePutFinalizeInOrder()
    {
        using var env = SeedEnv();
        var (client, twirp, factory) = BuildClient();

        using var workspace = new TempWorkspace();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "f.txt"), "hi");

        Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace.Path);
        try
        {
            await client.SaveCacheAsync(["f.txt"], "build-x");

            Assert.Single(twirp.CreateCalls);
            Assert.Single(twirp.FinalizeCalls);

            // The size_bytes is the actual zstd-compressed archive size on disk.
            Assert.True(int.TryParse(twirp.FinalizeCalls[0].SizeBytes, out var size) && size > 0);
            Assert.Equal(twirp.CreateCalls[0].Version, twirp.FinalizeCalls[0].Version);
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

    private static (DefaultCacheClient client, FakeCacheTwirpService twirp, FakeHttpClientFactory factory) BuildClient()
    {
        var twirp = new FakeCacheTwirpService();
        var handler = new TestHttpMessageHandler(req =>
        {
            // Drain PUT body so MockHttp-style streaming doesn't dispose early.
            if (req.Content is not null)
            {
                _ = req.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var factory = new FakeHttpClientFactory(handler);
        return (new DefaultCacheClient(twirp, factory), twirp, factory);
    }
}
