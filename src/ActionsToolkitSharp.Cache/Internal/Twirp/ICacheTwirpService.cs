// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Internal.Twirp;

/// <summary>
/// Internal Twirp transport for the V2 cache service. Mirrors the methods of
/// upstream <c>CacheServiceClient</c> in
/// <c>@actions/cache/src/internal/shared/cacheTwirpClient.ts</c>.
/// </summary>
internal interface ICacheTwirpService
{
    Task<CreateCacheEntryResponse> CreateCacheEntryAsync(
        CreateCacheEntryRequest request, CancellationToken cancellationToken = default);

    Task<FinalizeCacheEntryUploadResponse> FinalizeCacheEntryUploadAsync(
        FinalizeCacheEntryUploadRequest request, CancellationToken cancellationToken = default);

    Task<GetCacheEntryDownloadUrlResponse> GetCacheEntryDownloadUrlAsync(
        GetCacheEntryDownloadUrlRequest request, CancellationToken cancellationToken = default);

    Task<DeleteCacheEntryResponse> DeleteCacheEntryAsync(
        DeleteCacheEntryRequest request, CancellationToken cancellationToken = default);

    Task<ListCacheEntriesResponse> ListCacheEntriesAsync(
        ListCacheEntriesRequest request, CancellationToken cancellationToken = default);

    Task<LookupCacheEntryResponse> LookupCacheEntryAsync(
        LookupCacheEntryRequest request, CancellationToken cancellationToken = default);
}
