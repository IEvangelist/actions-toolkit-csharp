// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// cross-os-archive.cs
//
// Mirrors the upstream `@actions/cache` README "enableCrossOsArchive"
// flag: https://github.com/actions/toolkit/tree/main/packages/cache#options
//
// Saves and restores a cache that is portable across operating systems by
// passing `enableCrossOsArchive: true`, which omits the `windows-only`
// marker from the cache version hash so a cache produced on Windows can be
// restored on Linux / macOS (and vice-versa).
//
// Run with:
//   ACTIONS_CACHE_SERVICE_V2=1 \
//   ACTIONS_RESULTS_URL=https://results... \
//   ACTIONS_RUNTIME_TOKEN=ghs_xxx \
//   GITHUB_WORKSPACE=$(pwd) \
//   dotnet run cross-os-archive.cs build-shared-abc123

#:package ActionsToolkitSharp.Cache@*
#:package Microsoft.Extensions.DependencyInjection@*
#:package Microsoft.Extensions.Http@*

using ActionsToolkitSharp.Cache;
using ActionsToolkitSharp.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

var key = args is [var k, ..]
    ? k
    : throw new ArgumentException("Usage: dotnet run cross-os-archive.cs <key>");

using var provider = new ServiceCollection()
    .AddCacheServices()
    .BuildServiceProvider();

var cache = provider.GetRequiredService<ICacheClient>();

string[] paths = ["bin"];

var restored = await cache.RestoreCacheAsync(
    paths,
    key,
    restoreKeys: null,
    options: null,
    enableCrossOsArchive: true);

if (restored is not null)
{
    Console.WriteLine($"Restored shared cache {restored}.");
    return;
}

Directory.CreateDirectory("bin");
await File.WriteAllTextAsync(Path.Combine("bin", "shared.txt"), "shared output\n");

var saved = await cache.SaveCacheAsync(
    paths,
    key,
    options: null,
    enableCrossOsArchive: true);

Console.WriteLine($"Saved shared cache id={saved?.Key} size={saved?.Size}.");
