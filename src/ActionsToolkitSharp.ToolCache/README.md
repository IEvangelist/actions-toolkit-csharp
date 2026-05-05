# `ActionsToolkitSharp.ToolCache` package

To install the [`ActionsToolkitSharp.ToolCache`](https://www.nuget.org/packages/ActionsToolkitSharp.ToolCache) NuGet package:

```xml
<PackageReference Include="ActionsToolkitSharp.ToolCache" Version="[Version]" />
```

Or use the [`dotnet add package`](https://learn.microsoft.com/dotnet/core/tools/dotnet-add-package) .NET CLI command:

```bash
dotnet add package ActionsToolkitSharp.ToolCache
```

## Get the `IToolCacheService` instance

`ActionsToolkitSharp.ToolCache` is the .NET equivalent of the official
[`@actions/tool-cache`][upstream] Node package. It exposes
`IToolCacheService` for downloading, extracting, caching, and resolving
tools on a self-hosted or hosted runner. Register the service with an
`IServiceCollection` by calling `AddGitHubActionsToolCache()` and consume
`IToolCacheService` via constructor dependency injection.

```csharp
using Microsoft.Extensions.DependencyInjection;
using ActionsToolkitSharp.ToolCache;
using ActionsToolkitSharp.ToolCache.Extensions;

using var provider = new ServiceCollection()
    .AddGitHubActionsToolCache()
    .BuildServiceProvider();

var toolCache = provider.GetRequiredService<IToolCacheService>();
```

The extension transitively registers `ActionsToolkitSharp.HttpClient` so
the `IHttpClient` used for downloads is fully resilient (Polly-backed).

## Usage

The runner sets two environment variables that this package consults:

| Variable | Purpose |
| --- | --- |
| `RUNNER_TOOL_CACHE` | Root directory of the on-disk tool cache (`<tool>/<version>/<arch>` layout). |
| `RUNNER_TEMP` | Scratch directory used for intermediate downloads/extractions. |

### Download

```csharp
var nodePath = await toolCache.DownloadToolAsync(
    "https://nodejs.org/dist/v20.10.0/node-v20.10.0-linux-x64.tar.gz");
```

### Extract

The .NET port uses [SharpCompress](https://github.com/adamhathcock/sharpcompress)
for tar/tar.gz/tar.xz/tar.bz2/zstd/7z extraction (no native `tar` /
`7z.exe` shell-out is required). For zip the BCL `System.IO.Compression`
is used.

```csharp
var extracted = await toolCache.ExtractTarAsync(nodePath);
// or:
var extracted = await toolCache.ExtractZipAsync(zipPath);
var extracted = await toolCache.Extract7zAsync(sevenZipPath);
```

`ExtractXarAsync` throws `PlatformNotSupportedException`: SharpCompress
does not ship xar support, so consumers should shell out to the native
`xar` binary on macOS instead.

### Cache

```csharp
var cached = await toolCache.CacheDirAsync(extracted, "node", "20.10.0");
// or cache a single file (e.g. a downloaded GUID) under a friendly name:
var cached = await toolCache.CacheFileAsync(downloaded, "node.exe", "node", "20.10.0");
```

### Find

```csharp
var nodeDir = toolCache.Find("node", "20.x");
var allVersions = toolCache.FindAllVersions("node");
```

### Versions manifest

```csharp
var manifest = await toolCache.GetManifestFromRepoAsync(
    owner: "actions",
    repo: "node-versions",
    authToken: token,
    branch: "main");

var release = await toolCache.FindFromManifestAsync("20.x", stable: true, manifest);
```

### Evaluate versions

```csharp
var versions = new[] { "1.0.0", "1.2.3", "2.0.0", "3.0.1-beta" };
var match = toolCache.EvaluateVersions(versions, "1.x"); // "1.2.3"
```

## Native AOT

The package is fully compatible with .NET Native AOT. JSON
(de)serialization is driven entirely by source-generated
`JsonSerializerContext`s — no reflection-based serializer overloads are
called, no dynamic code is generated. A dedicated AOT smoke-test
project (`tests/ActionsToolkitSharp.ToolCache.Aot.Tests`) publishes a
small consumer with `<PublishAot>true</PublishAot>` and exercises every
public API to guard against IL2026/IL3050 regressions.

## Attribution

This package is a .NET port of the official
[`@actions/tool-cache`][upstream] Node.js package by GitHub, licensed
under the [MIT License](https://github.com/actions/toolkit/blob/main/LICENSE.md).

Archive extraction is delegated to
[SharpCompress](https://github.com/adamhathcock/sharpcompress), licensed
under the [MIT License](https://github.com/adamhathcock/sharpcompress/blob/main/LICENSE.txt).

The npm-style semver subset is hand-rolled and modeled on
[node-semver](https://github.com/npm/node-semver), also under the MIT
License.

[upstream]: https://github.com/actions/toolkit/tree/main/packages/tool-cache
