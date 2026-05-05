// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// lookup-only.cs
//
// Mirrors the upstream `@actions/cache` README "lookupOnly" option:
// https://github.com/actions/toolkit/tree/main/packages/cache#options
//
// Calls RestoreCacheAsync with `LookupOnly = true` to detect a cache hit
// without downloading the archive — useful for pre-flight checks that
// decide whether expensive work should run.
//
// Run with:
//   ACTIONS_CACHE_SERVICE_V2=1 \
//   ACTIONS_RESULTS_URL=https://results... \
//   ACTIONS_RUNTIME_TOKEN=ghs_xxx \
//   dotnet run lookup-only.cs build-linux-abc123

#:package ActionsToolkitSharp.Cache@*
#:package Microsoft.Extensions.DependencyInjection@*
#:package Microsoft.Extensions.Http@*

using ActionsToolkitSharp.Cache;
using ActionsToolkitSharp.Cache.Models;
using ActionsToolkitSharp.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

var primaryKey = args is [var k, ..]
    ? k
    : throw new ArgumentException("Usage: dotnet run lookup-only.cs <primary-key>");

using var provider = new ServiceCollection()
    .AddCacheServices()
    .BuildServiceProvider();

var cache = provider.GetRequiredService<ICacheClient>();

var matched = await cache.RestoreCacheAsync(
    ["bin"],
    primaryKey,
    restoreKeys: null,
    options: new RestoreCacheOptions { LookupOnly = true });

Console.WriteLine(matched is null
    ? $"No cache exists for {primaryKey} — work needs to run."
    : $"Cache exists for {matched} — work can be skipped.");
