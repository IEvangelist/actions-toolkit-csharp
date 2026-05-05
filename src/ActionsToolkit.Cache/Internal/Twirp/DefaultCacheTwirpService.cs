// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Internal.Twirp;

/// <summary>
/// Default <see cref="ICacheTwirpService"/> that issues Twirp-over-HTTP RPCs
/// (POST <c>/twirp/github.actions.results.api.v1.CacheService/&lt;Method&gt;</c>)
/// against an <see cref="NetClient"/> whose <c>BaseAddress</c> and bearer
/// authorization header are configured at registration time from
/// <c>ACTIONS_RESULTS_URL</c> / <c>ACTIONS_RUNTIME_TOKEN</c>.
/// </summary>
internal sealed class DefaultCacheTwirpService : ICacheTwirpService
{
    /// <summary>
    /// The Twirp service identifier used to build the request path.
    /// </summary>
    internal const string CacheServiceName = "github.actions.results.api.v1.CacheService";

    /// <summary>
    /// The named <see cref="IHttpClientFactory"/> client name used by
    /// <c>AddCacheServices</c> on <see cref="IServiceCollection"/>.
    /// </summary>
    internal const string HttpClientName = "ActionsToolkit.Cache";

    private static CacheJsonContext JsonContext => CacheJsonContext.Default;

    private readonly NetClient _httpClient;

    public DefaultCacheTwirpService(NetClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    public Task<CreateCacheEntryResponse> CreateCacheEntryAsync(
        CreateCacheEntryRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "CreateCacheEntry",
            request,
            JsonContext.CreateCacheEntryRequest,
            JsonContext.CreateCacheEntryResponse,
            cancellationToken);

    public Task<FinalizeCacheEntryUploadResponse> FinalizeCacheEntryUploadAsync(
        FinalizeCacheEntryUploadRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "FinalizeCacheEntryUpload",
            request,
            JsonContext.FinalizeCacheEntryUploadRequest,
            JsonContext.FinalizeCacheEntryUploadResponse,
            cancellationToken);

    public Task<GetCacheEntryDownloadUrlResponse> GetCacheEntryDownloadUrlAsync(
        GetCacheEntryDownloadUrlRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "GetCacheEntryDownloadURL",
            request,
            JsonContext.GetCacheEntryDownloadUrlRequest,
            JsonContext.GetCacheEntryDownloadUrlResponse,
            cancellationToken);

    public Task<DeleteCacheEntryResponse> DeleteCacheEntryAsync(
        DeleteCacheEntryRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "DeleteCacheEntry",
            request,
            JsonContext.DeleteCacheEntryRequest,
            JsonContext.DeleteCacheEntryResponse,
            cancellationToken);

    public Task<ListCacheEntriesResponse> ListCacheEntriesAsync(
        ListCacheEntriesRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "ListCacheEntries",
            request,
            JsonContext.ListCacheEntriesRequest,
            JsonContext.ListCacheEntriesResponse,
            cancellationToken);

    public Task<LookupCacheEntryResponse> LookupCacheEntryAsync(
        LookupCacheEntryRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "LookupCacheEntry",
            request,
            JsonContext.LookupCacheEntryRequest,
            JsonContext.LookupCacheEntryResponse,
            cancellationToken);

    private async Task<TResponse> InvokeAsync<TRequest, TResponse>(
        string method,
        TRequest data,
        JsonTypeInfo<TRequest> requestJsonTypeInfo,
        JsonTypeInfo<TResponse> responseJsonTypeInfo,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(data);

        var requestUri = $"/twirp/{CacheServiceName}/{method}";

        using var response = await _httpClient.PostAsJsonAsync(
            requestUri,
            data,
            requestJsonTypeInfo,
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new CacheServiceException(
                $"{method}: backend returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        var result = await response.Content.ReadFromJsonAsync(
            responseJsonTypeInfo,
            cancellationToken).ConfigureAwait(false);

        return result ?? throw new CacheServiceException(
            $"{method}: backend returned an empty response body.");
    }
}
