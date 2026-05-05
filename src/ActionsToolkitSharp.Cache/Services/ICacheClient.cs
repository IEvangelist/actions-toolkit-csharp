// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Services;

/// <summary>
/// Client for saving and restoring tarballs against the GitHub Actions cache
/// service. Mirrors the public surface of upstream
/// <see href="https://github.com/actions/toolkit/tree/main/packages/cache">@actions/cache</see>
/// (<c>saveCache</c> / <c>restoreCache</c> / <c>isFeatureAvailable</c>).
/// </summary>
public interface ICacheClient
{
    /// <summary>
    /// Saves a list of files / directories to the cache under
    /// <paramref name="key"/>. The default V2 path uses Twirp +
    /// signed-URL upload. Returns the resulting <see cref="CacheEntry"/>
    /// (containing the entry id, key, version, size, creation timestamp) or
    /// <c>null</c> when the save was skipped (e.g. when no files matched).
    /// </summary>
    /// <param name="paths">The list of file or directory paths to cache.</param>
    /// <param name="key">The key the cache will be stored under (≤ 512 chars,
    /// no commas).</param>
    /// <param name="options">Optional upload options (concurrency, chunk size).</param>
    /// <param name="enableCrossOsArchive">When true, omit the
    /// <c>windows-only</c> marker from the version hash so a cache saved on
    /// Windows can be restored on Linux / macOS (and vice-versa).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <exception cref="CacheValidationException"><paramref name="paths"/> is
    /// empty, or <paramref name="key"/> exceeds 512 chars / contains commas.</exception>
    /// <exception cref="ReserveCacheException">Cache reservation
    /// (<c>CreateCacheEntry</c>) failed.</exception>
    /// <exception cref="FinalizeCacheException">Cache finalization
    /// (<c>FinalizeCacheEntryUpload</c>) failed.</exception>
    /// <exception cref="CacheServiceUnavailableException">The cache service
    /// URL env var is not set.</exception>
    ValueTask<CacheEntry?> SaveCacheAsync(
        IReadOnlyList<string> paths,
        string key,
        SaveCacheOptions? options = null,
        bool enableCrossOsArchive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a cache by primary key, then by each restore key in order.
    /// Returns the matched key on a hit, or <c>null</c> on a miss.
    /// </summary>
    /// <param name="paths">The list of file or directory paths the cache covers.</param>
    /// <param name="primaryKey">The exact key to look up first.</param>
    /// <param name="restoreKeys">An ordered list of fallback prefixes (≤ 9
    /// entries; combined with <paramref name="primaryKey"/> this must not
    /// exceed 10 keys).</param>
    /// <param name="options">Optional download options
    /// (<see cref="RestoreCacheOptions.LookupOnly"/>, etc.).</param>
    /// <param name="enableCrossOsArchive">When true, allow restoring an
    /// archive saved on a different OS.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <exception cref="CacheValidationException">Validation of paths or
    /// keys failed.</exception>
    /// <exception cref="CacheServiceUnavailableException">The cache service
    /// URL env var is not set.</exception>
    ValueTask<string?> RestoreCacheAsync(
        IReadOnlyList<string> paths,
        string primaryKey,
        IReadOnlyList<string>? restoreKeys = null,
        RestoreCacheOptions? options = null,
        bool enableCrossOsArchive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true when the GitHub Actions cache service is available — the
    /// process must be running inside a GitHub-hosted runner job that
    /// exposed <c>ACTIONS_RESULTS_URL</c> (V2) or <c>ACTIONS_CACHE_URL</c>
    /// (V1). Mirrors upstream <c>isFeatureAvailable()</c>.
    /// </summary>
    bool IsFeatureAvailable();
}
