// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Internal.Twirp;

internal sealed class CreateCacheEntryRequest
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

internal sealed class CreateCacheEntryResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("signed_upload_url")]
    public string? SignedUploadUrl { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}

internal sealed class FinalizeCacheEntryUploadRequest
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("size_bytes")]
    public required string SizeBytes { get; init; }
}

internal sealed class FinalizeCacheEntryUploadResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("entry_id")]
    public string? EntryId { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}

internal sealed class GetCacheEntryDownloadUrlRequest
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("restore_keys")]
    public IReadOnlyList<string>? RestoreKeys { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

internal sealed class GetCacheEntryDownloadUrlResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("signed_download_url")]
    public string? SignedDownloadUrl { get; init; }

    [JsonPropertyName("matched_key")]
    public string? MatchedKey { get; init; }
}

internal sealed class DeleteCacheEntryRequest
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }
}

internal sealed class DeleteCacheEntryResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("entry_id")]
    public string? EntryId { get; init; }
}

internal sealed class ListCacheEntriesRequest
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("restore_keys")]
    public IReadOnlyList<string>? RestoreKeys { get; init; }
}

internal sealed class ListCacheEntriesResponse
{
    [JsonPropertyName("entries")]
    public IReadOnlyList<CacheEntryDto> Entries { get; init; } = [];
}

internal sealed class CacheEntryDto
{
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    [JsonPropertyName("size_bytes")]
    public string? SizeBytes { get; init; }

    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }
}

internal sealed class LookupCacheEntryRequest
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("restore_keys")]
    public IReadOnlyList<string>? RestoreKeys { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

internal sealed class LookupCacheEntryResponse
{
    [JsonPropertyName("exists")]
    public bool Exists { get; init; }

    [JsonPropertyName("entry")]
    public CacheEntryDto? Entry { get; init; }
}
