// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Services;

/// <summary>
/// Default <see cref="ICacheClient"/> orchestrator. The V2 path:
/// <list type="number">
///   <item>tar+zstd the cache paths into a temp archive</item>
///   <item>POST <c>CreateCacheEntry</c> Twirp → returns signed upload URL</item>
///   <item>PUT the archive to the signed URL with no auth</item>
///   <item>POST <c>FinalizeCacheEntryUpload</c> Twirp → returns entry id</item>
/// </list>
/// On restore, the same shape but with <c>GetCacheEntryDownloadURL</c> and
/// GET against the signed URL. Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/src/cache.ts">cache.ts</see>.
/// </summary>
internal sealed class DefaultCacheClient : ICacheClient
{
    /// <summary>
    /// The named <see cref="IHttpClientFactory"/> client used for blob
    /// upload/download against the signed URLs returned by the cache
    /// service. No authorization header — the URL itself is presigned.
    /// </summary>
    internal const string BlobHttpClientName = "ActionsToolkitSharp.Cache.Blob";

    private readonly ICacheTwirpService _twirp;
    private readonly IHttpClientFactory _httpClientFactory;

    public DefaultCacheClient(
        ICacheTwirpService twirp,
        IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(twirp);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _twirp = twirp;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public bool IsFeatureAvailable() => CacheUtils.IsFeatureAvailable();

    /// <inheritdoc />
    public async ValueTask<CacheEntry?> SaveCacheAsync(
        IReadOnlyList<string> paths,
        string key,
        SaveCacheOptions? options = null,
        bool enableCrossOsArchive = false,
        CancellationToken cancellationToken = default)
    {
        CacheUtils.CheckPaths(paths);
        CacheUtils.CheckKey(key);

        EnsureCacheServiceConfigured();

        var workspace = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
        if (string.IsNullOrEmpty(workspace))
        {
            workspace = Environment.CurrentDirectory;
        }

        const CompressionMethod method = CompressionMethod.ZstdWithoutLong;
        var archiveFolder = CacheUtils.CreateTempDirectory();
        var archivePath = Path.Combine(archiveFolder, CacheUtils.GetCacheFileName(method));

        try
        {
            await CacheTar.CreateAsync(
                archivePath, workspace, paths, method, enableCrossOsArchive, cancellationToken)
                .ConfigureAwait(false);

            var archiveSize = CacheUtils.GetArchiveFileSizeInBytes(archivePath);
            var version = CacheUtils.GetCacheVersion(paths, method, enableCrossOsArchive);

            // CreateCacheEntry — reserves the cache and returns a signed
            // upload URL. A non-ok response is mapped to ReserveCacheException
            // so callers can distinguish reservation failures from other errors.
            var createResponse = await _twirp.CreateCacheEntryAsync(
                new CreateCacheEntryRequest { Key = key, Version = version },
                cancellationToken).ConfigureAwait(false);

            if (!createResponse.Ok || string.IsNullOrEmpty(createResponse.SignedUploadUrl))
            {
                throw new ReserveCacheException(
                    $"Unable to reserve cache with key {key}. {createResponse.Message}".TrimEnd());
            }

            await UploadAsync(createResponse.SignedUploadUrl, archivePath, cancellationToken)
                .ConfigureAwait(false);

            var finalizeResponse = await _twirp.FinalizeCacheEntryUploadAsync(
                new FinalizeCacheEntryUploadRequest
                {
                    Key = key,
                    Version = version,
                    SizeBytes = archiveSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                },
                cancellationToken).ConfigureAwait(false);

            if (!finalizeResponse.Ok)
            {
                throw new FinalizeCacheException(
                    finalizeResponse.Message
                    ?? $"Unable to finalize cache with key {key}, another job may be finalizing this cache.");
            }

            return new CacheEntry(
                Key: key,
                Version: version,
                Size: archiveSize,
                CreatedAt: DateTimeOffset.UtcNow);
        }
        finally
        {
            CacheUtils.TryDelete(archivePath);
            try { Directory.Delete(archiveFolder, recursive: true); }
            catch { /* best effort */ }
        }
    }

    /// <inheritdoc />
    public async ValueTask<string?> RestoreCacheAsync(
        IReadOnlyList<string> paths,
        string primaryKey,
        IReadOnlyList<string>? restoreKeys = null,
        RestoreCacheOptions? options = null,
        bool enableCrossOsArchive = false,
        CancellationToken cancellationToken = default)
    {
        CacheUtils.CheckPaths(paths);
        ArgumentException.ThrowIfNullOrEmpty(primaryKey);

        var keys = new List<string>(1 + (restoreKeys?.Count ?? 0)) { primaryKey };
        if (restoreKeys is not null)
        {
            keys.AddRange(restoreKeys);
        }

        if (keys.Count > CacheUtils.MaxRestoreKeys)
        {
            throw new CacheValidationException(
                $"Key Validation Error: Keys are limited to a maximum of {CacheUtils.MaxRestoreKeys}.");
        }

        foreach (var key in keys)
        {
            CacheUtils.CheckKey(key);
        }

        EnsureCacheServiceConfigured();

        const CompressionMethod method = CompressionMethod.ZstdWithoutLong;
        var version = CacheUtils.GetCacheVersion(paths, method, enableCrossOsArchive);

        var response = await _twirp.GetCacheEntryDownloadUrlAsync(
            new GetCacheEntryDownloadUrlRequest
            {
                Key = primaryKey,
                RestoreKeys = restoreKeys,
                Version = version,
            },
            cancellationToken).ConfigureAwait(false);

        if (!response.Ok || string.IsNullOrEmpty(response.SignedDownloadUrl))
        {
            return null;
        }

        if (options?.LookupOnly == true)
        {
            return response.MatchedKey;
        }

        var workspace = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
        if (string.IsNullOrEmpty(workspace))
        {
            workspace = Environment.CurrentDirectory;
        }

        var archiveFolder = CacheUtils.CreateTempDirectory();
        var archivePath = Path.Combine(archiveFolder, CacheUtils.GetCacheFileName(method));

        try
        {
            await DownloadAsync(response.SignedDownloadUrl, archivePath, cancellationToken)
                .ConfigureAwait(false);

            await CacheTar.ExtractAsync(archivePath, workspace, method, cancellationToken)
                .ConfigureAwait(false);

            return response.MatchedKey;
        }
        finally
        {
            CacheUtils.TryDelete(archivePath);
            try { Directory.Delete(archiveFolder, recursive: true); }
            catch { /* best effort */ }
        }
    }

    private async Task UploadAsync(
        string signedUploadUrl,
        string archivePath,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(BlobHttpClientName);

        await using var fs = File.OpenRead(archivePath);
        using var content = new StreamContent(fs);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.ContentLength = fs.Length;

        using var request = new HttpRequestMessage(HttpMethod.Put, signedUploadUrl)
        {
            Content = content,
        };
        request.Headers.TryAddWithoutValidation("x-ms-blob-type", "BlockBlob");

        using var response = await client.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new CacheServiceException(
                $"Cache upload failed with status {(int)response.StatusCode} {response.ReasonPhrase}.");
        }
    }

    private async Task DownloadAsync(
        string signedDownloadUrl,
        string archivePath,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(BlobHttpClientName);

        using var response = await client.GetAsync(
            signedDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new CacheServiceException(
                $"Cache download failed with status {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        await using var src = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var fs = File.Create(archivePath);
        await src.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
    }

    private static void EnsureCacheServiceConfigured()
    {
        if (!CacheUtils.IsFeatureAvailable())
        {
            throw new CacheServiceUnavailableException(
                "Cache service URL is not configured. Ensure the runner exposes "
                + "ACTIONS_RESULTS_URL (v2) or ACTIONS_CACHE_URL (v1).");
        }
    }
}
