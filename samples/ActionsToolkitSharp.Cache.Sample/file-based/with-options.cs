// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// with-options.cs
//
// Mirrors the upstream `@actions/cache` README "options" surface:
// https://github.com/actions/toolkit/tree/main/packages/cache#options
//
// Demonstrates passing `SaveCacheOptions` (upload concurrency / chunk size)
// and `RestoreCacheOptions` (download concurrency / segment timeout). These
// shapes are passed through to the signed-URL transport when the underlying
// SDK supports tuning them.
//
// Run with:
//   ACTIONS_CACHE_SERVICE_V2=1 \
//   ACTIONS_RESULTS_URL=https://results... \
//   ACTIONS_RUNTIME_TOKEN=ghs_xxx \
//   GITHUB_WORKSPACE=$(pwd) \
//   dotnet run with-options.cs build-linux-abc123

#:package ActionsToolkitSharp.Cache@*
#:package Microsoft.Extensions.DependencyInjection@*
#:package Microsoft.Extensions.Http@*

using ActionsToolkitSharp.Cache;
using ActionsToolkitSharp.Cache.Models;
using ActionsToolkitSharp.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

var key = args is [var k, ..]
    ? k
    : throw new ArgumentException("Usage: dotnet run with-options.cs <key>");

using var provider = new ServiceCollection()
    .AddCacheServices()
    .BuildServiceProvider();

var cache = provider.GetRequiredService<ICacheClient>();

string[] paths = ["bin"];

var restoreOptions = new RestoreCacheOptions
{
    DownloadConcurrency = 8,
    TimeoutInMs = 30_000,
    SegmentTimeoutInMs = 600_000,
};

var restored = await cache.RestoreCacheAsync(paths, key, restoreKeys: null, options: restoreOptions);
if (restored is not null)
{
    Console.WriteLine($"Restored {restored} with concurrency=8.");
    return;
}

Directory.CreateDirectory("bin");
await File.WriteAllTextAsync(Path.Combine("bin", "out.txt"), "rebuild\n");

var saveOptions = new SaveCacheOptions
{
    UploadConcurrency = 8,
    UploadChunkSize = 32 * 1024 * 1024,
};

var saved = await cache.SaveCacheAsync(paths, key, options: saveOptions);
Console.WriteLine($"Saved cache id={saved?.Key} with concurrency=8 chunk=32MB.");
