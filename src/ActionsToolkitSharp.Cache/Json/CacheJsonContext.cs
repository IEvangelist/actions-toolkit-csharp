// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkitSharp.Cache.Json;
#pragma warning restore IDE0130

[JsonSerializable(typeof(CreateCacheEntryRequest))]
[JsonSerializable(typeof(CreateCacheEntryResponse))]
[JsonSerializable(typeof(FinalizeCacheEntryUploadRequest))]
[JsonSerializable(typeof(FinalizeCacheEntryUploadResponse))]
[JsonSerializable(typeof(GetCacheEntryDownloadUrlRequest))]
[JsonSerializable(typeof(GetCacheEntryDownloadUrlResponse))]
[JsonSerializable(typeof(DeleteCacheEntryRequest))]
[JsonSerializable(typeof(DeleteCacheEntryResponse))]
[JsonSerializable(typeof(ListCacheEntriesRequest))]
[JsonSerializable(typeof(ListCacheEntriesResponse))]
[JsonSerializable(typeof(LookupCacheEntryRequest))]
[JsonSerializable(typeof(LookupCacheEntryResponse))]
[JsonSerializable(typeof(CacheEntryDto))]
[JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.AllowReadingFromString)]
internal sealed partial class CacheJsonContext : JsonSerializerContext;
