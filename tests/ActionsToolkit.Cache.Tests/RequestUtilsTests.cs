// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Tests;

/// <summary>
/// Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/__tests__/requestUtils.test.ts">
/// <c>__tests__/requestUtils.test.ts</c></see>. The TypeScript module
/// implements a generic <c>retry()</c> / <c>retryTypedResponse()</c>
/// loop with an <c>isRetryable(statusCode)</c> predicate, max-attempts,
/// and an error-to-response converter.
/// <para>
/// The C# port routes every Twirp call through
/// <see cref="DefaultCacheTwirpService"/>, which currently issues a
/// single <c>POST</c> and surfaces non-2xx responses immediately as
/// <see cref="CacheServiceException"/> with no retry/back-off layer.
/// Documented parity gap:
/// </para>
/// <list type="bullet">
///   <item>No retry loop on transient 5xx — upstream retries on 408/500/502/503/504
///     up to <c>maxAttempts</c>; the C# transport throws on the first
///     non-success response. Future work: layer
///     <c>Microsoft.Extensions.Http.Resilience</c> on the named client.</item>
///   <item>No <c>retryTypedResponse</c> error-shape converter — upstream
///     converts <c>HttpClientError</c>s into a typed response with
///     <c>statusCode</c> / <c>result=null</c>; the C# port simply rethrows.</item>
/// </list>
/// The tests below pin the <em>current</em> single-attempt behavior so a
/// future resilience-policy addition deliberately updates this file.
/// </summary>
public sealed class RequestUtilsTests
{
    private const string ServiceName = "github.actions.results.api.v1.CacheService";

    private static StringContent JsonBody(string json) =>
        new(json, Encoding.UTF8, "application/json");

    [Fact(DisplayName = "Twirp returns parsed body on first 200 (mirrors 'retry works on successful response')")]
    public async Task Returns_OnFirst200()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonBody("""{ "ok": true, "signed_upload_url": "https://blob/u" }"""),
            });

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var response = await svc.CreateCacheEntryAsync(new CreateCacheEntryRequest
        {
            Key = "k",
            Version = "v",
        });

        Assert.True(response.Ok);
        Assert.Equal("https://blob/u", response.SignedUploadUrl);
        Assert.Single(handler.Requests);
    }

    [Fact(DisplayName = "Twirp throws on 503 without retry (parity-gap: upstream retries transient 5xx)")]
    public async Task Throws_OnTransient5xx_WithoutRetry()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var ex = await Assert.ThrowsAsync<CacheServiceException>(async () =>
            await svc.CreateCacheEntryAsync(new CreateCacheEntryRequest
            {
                Key = "k",
                Version = "v",
            }));

        Assert.Contains("503", ex.Message, StringComparison.Ordinal);
        // Single attempt — no retry loop yet (documented gap).
        Assert.Single(handler.Requests);
    }

    [Fact(DisplayName = "Twirp throws on 4xx (mirrors 'retry returns after client error' — non-retryable)")]
    public async Task Throws_OnClientError()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = JsonBody("""{ "msg": "bad request" }"""),
            });

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var ex = await Assert.ThrowsAsync<CacheServiceException>(async () =>
            await svc.CreateCacheEntryAsync(new CreateCacheEntryRequest
            {
                Key = "k",
                Version = "v",
            }));

        Assert.Contains("400", ex.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
    }

    [Fact(DisplayName = "Twirp surfaces non-2xx with method + status in the exception message")]
    public async Task ExceptionMessage_IncludesMethodAndStatus()
    {
        var handler = new TestHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.BadGateway));

        using var http = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("https://results.example.com/"),
        };
        var svc = new DefaultCacheTwirpService(http);

        var ex = await Assert.ThrowsAsync<CacheServiceException>(async () =>
            await svc.GetCacheEntryDownloadUrlAsync(new GetCacheEntryDownloadUrlRequest
            {
                Key = "k",
                Version = "v",
            }));

        Assert.Contains("GetCacheEntryDownloadURL", ex.Message, StringComparison.Ordinal);
        Assert.Contains("502", ex.Message, StringComparison.Ordinal);
        Assert.Equal($"/twirp/{ServiceName}/GetCacheEntryDownloadURL",
            handler.Requests[0].RequestUri!.AbsolutePath);
    }
}
