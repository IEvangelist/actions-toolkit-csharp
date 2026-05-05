// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// restore-and-save.cs
//
// Mirrors the upstream `@actions/cache` README "restore + save" pattern:
// https://github.com/actions/toolkit/tree/main/packages/cache#restore-cache
//
// Tries to restore a cache by primary key (with one fallback restoreKey),
// runs a fake build on a miss, then saves the resulting bin/ directory back
// to the cache.
//
// Run with:
//   ACTIONS_CACHE_SERVICE_V2=1 \
//   ACTIONS_RESULTS_URL=https://results... \
//   ACTIONS_RUNTIME_TOKEN=ghs_xxx \
//   GITHUB_WORKSPACE=$(pwd) \
//   dotnet run restore-and-save.cs build-linux-abc123

#:package ActionsToolkitSharp.Cache@*
#:package Microsoft.Extensions.DependencyInjection@*
#:package Microsoft.Extensions.Http@*

using ActionsToolkitSharp.Cache;
using ActionsToolkitSharp.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

var primaryKey = args is [var k, ..]
    ? k
    : throw new ArgumentException("Usage: dotnet run restore-and-save.cs <primary-key>");

using var provider = new ServiceCollection()
    .AddCacheServices()
    .BuildServiceProvider();

var cache = provider.GetRequiredService<ICacheClient>();

string[] paths = ["bin"];
string[] restoreKeys = ["build-linux-"];

var restored = await cache.RestoreCacheAsync(paths, primaryKey, restoreKeys);
if (restored is not null)
{
    Console.WriteLine($"Cache hit on {restored} — skipping rebuild.");
    return;
}

Console.WriteLine($"Cache miss for {primaryKey} — running fake build.");
Directory.CreateDirectory("bin");
await File.WriteAllTextAsync(Path.Combine("bin", "build-output.txt"), "compiled\n");

var saved = await cache.SaveCacheAsync(paths, primaryKey);
Console.WriteLine($"Saved cache id={saved?.Key} size={saved?.Size}.");
