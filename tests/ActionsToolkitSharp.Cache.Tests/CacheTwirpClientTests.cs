// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Unit tests for the internal Twirp transport. Mirrors upstream
/// <c>__tests__/cacheTwirpClient.test.ts</c>.
/// </summary>
public sealed class CacheTwirpClientTests
{
    private const string ServiceName = "github.actions.results.api.v1.CacheService";

    private static StringContent JsonBody(string json) =>
        new(json, Encoding.UTF8, "application/json");

    [Fact(DisplayName = "CreateCacheEntry posts to /twirp/.../CreateCacheEntry")]
    public async Task CreateCacheEntryPostsToTwirpUrl()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonBody("""{ "ok": true, "signed_upload_url": "https://blob.example.com/upload?sig=abc" }"""),
            });

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var response = await svc.CreateCacheEntryAsync(new CreateCacheEntryRequest
        {
            Key = "build-x",
            Version = "v",
        });

        Assert.True(response.Ok);
        Assert.Equal("https://blob.example.com/upload?sig=abc", response.SignedUploadUrl);

        var captured = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, captured.Method);
        Assert.Equal($"/twirp/{ServiceName}/CreateCacheEntry", captured.RequestUri!.AbsolutePath);
    }

    [Fact(DisplayName = "FinalizeCacheEntryUpload posts to /twirp/.../FinalizeCacheEntryUpload")]
    public async Task FinalizeCacheEntryUploadPostsToTwirpUrl()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonBody("""{ "ok": true, "entry_id": "42" }"""),
            });

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var response = await svc.FinalizeCacheEntryUploadAsync(new FinalizeCacheEntryUploadRequest
        {
            Key = "k",
            Version = "v",
            SizeBytes = "100",
        });

        Assert.True(response.Ok);
        Assert.Equal("42", response.EntryId);
        Assert.Equal($"/twirp/{ServiceName}/FinalizeCacheEntryUpload",
            handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact(DisplayName = "GetCacheEntryDownloadURL posts to /twirp/.../GetCacheEntryDownloadURL")]
    public async Task GetCacheEntryDownloadUrlPostsToTwirpUrl()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonBody("""{ "ok": true, "signed_download_url": "https://blob.example.com/dl", "matched_key": "build-x" }"""),
            });

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var response = await svc.GetCacheEntryDownloadUrlAsync(new GetCacheEntryDownloadUrlRequest
        {
            Key = "build-x",
            Version = "v",
        });

        Assert.True(response.Ok);
        Assert.Equal("build-x", response.MatchedKey);
        Assert.Equal($"/twirp/{ServiceName}/GetCacheEntryDownloadURL",
            handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact(DisplayName = "DeleteCacheEntry posts to /twirp/.../DeleteCacheEntry")]
    public async Task DeleteCacheEntryPostsToTwirpUrl()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonBody("""{ "ok": true, "entry_id": "42" }"""),
            });

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var response = await svc.DeleteCacheEntryAsync(new DeleteCacheEntryRequest { Key = "k" });

        Assert.True(response.Ok);
        Assert.Equal($"/twirp/{ServiceName}/DeleteCacheEntry",
            handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact(DisplayName = "ListCacheEntries posts to /twirp/.../ListCacheEntries")]
    public async Task ListCacheEntriesPostsToTwirpUrl()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonBody("""{ "entries": [ { "key": "k", "hash": "h", "size_bytes": "1", "scope": "s", "version": "v" } ] }"""),
            });

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var response = await svc.ListCacheEntriesAsync(new ListCacheEntriesRequest { Key = "k" });

        Assert.Single(response.Entries);
        Assert.Equal("k", response.Entries[0].Key);
    }

    [Fact(DisplayName = "LookupCacheEntry posts to /twirp/.../LookupCacheEntry")]
    public async Task LookupCacheEntryPostsToTwirpUrl()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonBody("""{ "exists": true }"""),
            });

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var response = await svc.LookupCacheEntryAsync(new LookupCacheEntryRequest
        {
            Key = "k",
            Version = "v",
        });

        Assert.True(response.Exists);
    }

    [Fact(DisplayName = "Twirp non-2xx responses surface as CacheServiceException")]
    public async Task TwirpNonSuccessThrows()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.BadGateway));

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        await Assert.ThrowsAsync<CacheServiceException>(async () =>
            await svc.CreateCacheEntryAsync(new CreateCacheEntryRequest
            {
                Key = "k",
                Version = "v",
            }));
    }

    [Fact(DisplayName = "Twirp serializes snake_case JSON property names")]
    public async Task TwirpSerializesSnakeCase()
    {
        string? capturedBody = null;
        var handler = new TestHttpMessageHandler((req, ct) =>
        {
            capturedBody = req.Content!.ReadAsStringAsync(ct).GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonBody("""{ "ok": true }"""),
            };
        });

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        await svc.FinalizeCacheEntryUploadAsync(new FinalizeCacheEntryUploadRequest
        {
            Key = "k",
            Version = "v",
            SizeBytes = "42",
        });

        Assert.NotNull(capturedBody);
        Assert.Contains("\"size_bytes\":\"42\"", capturedBody, StringComparison.Ordinal);
        Assert.Contains("\"key\":\"k\"", capturedBody, StringComparison.Ordinal);
        Assert.Contains("\"version\":\"v\"", capturedBody, StringComparison.Ordinal);
    }
}
