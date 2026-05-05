// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// download-and-cache.cs
//
// Mirrors the upstream `@actions/tool-cache` README "Download" + "Extract" +
// "Cache" snippets:
// https://github.com/actions/toolkit/tree/main/packages/tool-cache#download
//
// Downloads a tool to RUNNER_TEMP, extracts it, and caches the result under
// RUNNER_TOOL_CACHE/<tool>/<version>/<arch>. Subsequent runs of `Find`
// return the cached path.
//
// Run with:
//   RUNNER_TEMP=$(mktemp -d) RUNNER_TOOL_CACHE=$(mktemp -d) dotnet run download-and-cache.cs

#:package ActionsToolkit.ToolCache@*
#:package ActionsToolkit.HttpClient@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.ToolCache;
using ActionsToolkit.ToolCache.Extensions;
using Microsoft.Extensions.DependencyInjection;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RUNNER_TEMP")) ||
    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RUNNER_TOOL_CACHE")))
{
    Console.Error.WriteLine(
        "RUNNER_TEMP and RUNNER_TOOL_CACHE must be set. Try:");
    Console.Error.WriteLine(
        "  RUNNER_TEMP=$(mktemp -d) RUNNER_TOOL_CACHE=$(mktemp -d) dotnet run download-and-cache.cs");
    Environment.Exit(1);
}

using var provider = new ServiceCollection()
    .AddGitHubActionsToolCache()
    .BuildServiceProvider();

var toolCache = provider.GetRequiredService<IToolCacheService>();

// Pick a tiny, predictable artifact: the upstream toolkit tarball itself.
const string toolName = "actions-toolkit-sample-toolkit";
const string version = "1.0.0";
const string url = "https://github.com/actions/toolkit/archive/refs/tags/v0.1.0.tar.gz";

Console.WriteLine($"Downloading {url} ...");
var downloaded = await toolCache.DownloadToolAsync(url);
Console.WriteLine($"  saved to {downloaded}");

Console.WriteLine("Extracting ...");
var extracted = await toolCache.ExtractTarAsync(downloaded);
Console.WriteLine($"  extracted to {extracted}");

Console.WriteLine($"Caching as {toolName}@{version} ...");
var cached = await toolCache.CacheDirAsync(extracted, toolName, version);
Console.WriteLine($"  cached at {cached}");

var hit = toolCache.Find(toolName, version);
Console.WriteLine($"Find('{toolName}', '{version}') -> {hit}");
