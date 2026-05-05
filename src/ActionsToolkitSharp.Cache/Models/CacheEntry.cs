// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Models;

/// <summary>
/// Describes a cache entry returned by
/// <see cref="ICacheClient.SaveCacheAsync(System.Collections.Generic.IReadOnlyList{string}, string, SaveCacheOptions?, bool, System.Threading.CancellationToken)"/>.
/// </summary>
/// <param name="Key">The key the cache was saved under.</param>
/// <param name="Version">The cache version (sha256 hash of the paths plus
/// compression method and platform marker). Used by the service to scope
/// cache reads to compatible archives.</param>
/// <param name="Size">The size of the compressed archive in bytes.</param>
/// <param name="CreatedAt">UTC timestamp when the cache was created.</param>
public sealed record CacheEntry(
    string Key,
    string Version,
    long Size,
    DateTimeOffset CreatedAt);
