# ActionsToolkit.Cache

[![NuGet](https://img.shields.io/nuget/v/ActionsToolkit.Cache?label=ActionsToolkit.Cache&logo=nuget)](https://www.nuget.org/packages/ActionsToolkit.Cache)

An unofficial .NET SDK for GitHub Actions workflows, providing a port of the
[`@actions/cache`](https://github.com/actions/toolkit/tree/main/packages/cache)
JavaScript package. Use it from a custom action (or a workflow-driven .NET
process running on a GitHub Actions runner) to **save** a list of files /
directories under a key and **restore** them on a later run.

## Install

```xml
<PackageReference Include="ActionsToolkit.Cache" Version="[Version]" />
```

Or use the [`dotnet add package`](https://learn.microsoft.com/dotnet/core/tools/dotnet-add-package) .NET CLI command:

```bash
dotnet add package ActionsToolkit.Cache
```

## Get the `ICacheClient` instance

Register the services with an `IServiceCollection` instance by calling
`AddCacheServices()`, and consuming code can require the `ICacheClient` via
constructor dependency injection.

```csharp
using ActionsToolkit.Cache;
using ActionsToolkit.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddCacheServices()
    .BuildServiceProvider();

var cache = provider.GetRequiredService<ICacheClient>();
```

`AddCacheServices()` reads `ACTIONS_RESULTS_URL` and `ACTIONS_RUNTIME_TOKEN`
(set automatically by the runner) at the time each `HttpClient` is
materialized; you don't need to configure them explicitly. Call
`cache.IsFeatureAvailable()` if you want to short-circuit gracefully when
the env vars are absent.

## Save a cache

```csharp
var entry = await cache.SaveCacheAsync(
    paths: ["bin", "obj"],
    key: $"build-{Environment.OSVersion.Platform}-{commitSha}");

if (entry is not null)
{
    Console.WriteLine($"Saved cache {entry.Key} ({entry.Size} bytes)");
}
```

The client tar+zstd-compresses the listed paths, reserves a cache entry via
the V2 Twirp `CreateCacheEntry` RPC, PUTs the archive to the signed upload
URL, and finalizes the entry with `FinalizeCacheEntryUpload`. Compression
uses the [`ZstdSharp.Port`](https://github.com/oleg-st/ZstdSharp) AOT-clean
managed zstd implementation — there is **no** dependency on a native `zstd`
binary, `tar` binary, or the Azure Storage Blobs SDK.

## Restore a cache

```csharp
string? matchedKey = await cache.RestoreCacheAsync(
    paths: ["bin", "obj"],
    primaryKey: $"build-{Environment.OSVersion.Platform}-{commitSha}",
    restoreKeys: [$"build-{Environment.OSVersion.Platform}-"]);

if (matchedKey is not null)
{
    Console.WriteLine($"Cache hit: {matchedKey}");
}
```

`restoreKeys` are searched in order after the primary key misses; the
first prefix-match wins. The matched key is returned (which may equal
`primaryKey` for an exact hit, or one of the restore keys for a fallback
hit). On miss, `null` is returned.

### Lookup-only

```csharp
var key = await cache.RestoreCacheAsync(
    paths,
    primaryKey,
    restoreKeys,
    options: new RestoreCacheOptions { LookupOnly = true });
```

When `LookupOnly = true`, the client checks whether a matching entry
exists and returns the matched key without downloading or extracting the
archive — useful in jobs that only need to know whether a follow-up step
should rebuild.

### Cross-OS archives

By default, a cache saved on Windows can only be restored on Windows
(the OS marker is mixed into the version hash). Set
`enableCrossOsArchive: true` on both `SaveCacheAsync` and
`RestoreCacheAsync` to produce a portable archive.

## Key & path validation

| Rule | Source |
| --- | --- |
| At least one path | `CacheValidationException` |
| Key ≤ 512 chars | `CacheValidationException` |
| Key contains no commas | `CacheValidationException` |
| ≤ 10 keys total (primary + restore) | `CacheValidationException` |

## Errors

| Exception | Thrown when |
| --- | --- |
| `CacheValidationException` | A key or path violates the input rules above. |
| `ReserveCacheException` | `CreateCacheEntry` returned `ok=false` (another job may already own the key). |
| `FinalizeCacheException` | `FinalizeCacheEntryUpload` returned `ok=false` after the blob upload. |
| `CacheServiceUnavailableException` | `ACTIONS_RESULTS_URL` (V2) / `ACTIONS_CACHE_URL` (V1) is unset. |
| `CacheServiceException` | The Twirp service or signed-URL transport returned an unexpected status / payload. |

## Environment

| Variable | Purpose |
| --- | --- |
| `ACTIONS_RESULTS_URL` | Base URL of the V2 cache service (set by the runner). |
| `ACTIONS_RUNTIME_TOKEN` | Bearer token used for Twirp calls. |
| `ACTIONS_CACHE_SERVICE_V2` | When set, opt in to the V2 service (default on hosted runners). |
| `GITHUB_WORKSPACE` | Root used to resolve relative paths during tar create / extract. |
| `RUNNER_TEMP` | Optional override for the temp folder used to stage archives. |

## Architecture

The package decomposes the cache flows into injectable seams:

- **`ICacheClient`** — the public façade. The DI default
  (`DefaultCacheClient`) routes save through `CreateCacheEntry` →
  signed-URL `PUT` → `FinalizeCacheEntryUpload`, and restore through
  `GetCacheEntryDownloadURL` → signed-URL `GET` → tar extract.
- **`ICacheTwirpService`** *(internal)* — Twirp transport. Issues
  `POST /twirp/github.actions.results.api.v1.CacheService/<Method>`
  requests with the runtime token bearer.
- **`CacheTar`** *(internal)* — tar+zstd archive create / extract using
  `System.Formats.Tar` (built-in) and `ZstdSharp.Port` (managed zstd).

The package registers two named `HttpClient` instances:

| Name | Purpose |
| --- | --- |
| `ActionsToolkit.Cache` | Twirp transport (bearer auth from `ACTIONS_RUNTIME_TOKEN`). |
| `ActionsToolkit.Cache.Blob` | Bare client for blob upload/download via signed URLs (no auth). |

## Native AOT

The package is fully Native AOT-clean. JSON serialization uses a
`[JsonSerializable]` source-gen context (`CacheJsonContext`); zstd
compression uses managed code (`ZstdSharp.Port`); tar uses the built-in
`System.Formats.Tar` API. There is no reflection, no `JsonSerializer`
overload that takes a `Type`, and no `Microsoft.IdentityModel.*`
dependency.

## Attribution

Based on the original Node.js
[`@actions/cache`](https://github.com/actions/toolkit/tree/main/packages/cache)
package by GitHub, licensed under
[Apache-2.0](https://github.com/actions/toolkit/blob/main/packages/cache/LICENSE.md).
The .NET port is MIT-licensed and is **not** an official GitHub product.

Compression is handled by the
[`ZstdSharp.Port`](https://github.com/oleg-st/ZstdSharp) managed zstd
library; tar archive support is provided by the built-in
[`System.Formats.Tar`](https://learn.microsoft.com/dotnet/api/system.formats.tar)
API in .NET 10.
