// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// find.cs
//
// Mirrors the upstream `@actions/tool-cache` README "Find" snippet:
// https://github.com/actions/toolkit/tree/main/packages/tool-cache#find
//
// Looks up a previously-cached tool. Accepts a semver range and returns the
// highest matching cached version, or an empty string if not found.
//
// Run with:
//   RUNNER_TOOL_CACHE=$(mktemp -d) dotnet run find.cs

#:package ActionsToolkit.ToolCache@*
#:package ActionsToolkit.HttpClient@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.ToolCache;
using ActionsToolkit.ToolCache.Extensions;
using ActionsToolkit.ToolCache.Layout;
using Microsoft.Extensions.DependencyInjection;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RUNNER_TOOL_CACHE")))
{
    Console.Error.WriteLine(
        "RUNNER_TOOL_CACHE must be set. Try:");
    Console.Error.WriteLine(
        "  RUNNER_TOOL_CACHE=$(mktemp -d) dotnet run find.cs");
    Environment.Exit(1);
}

using var provider = new ServiceCollection()
    .AddGitHubActionsToolCache()
    .BuildServiceProvider();

var toolCache = provider.GetRequiredService<IToolCacheService>();

// Stage two fake versions in the cache so this sample is self-contained.
var arch = ToolCacheLayout.GetDefaultArch();
foreach (var version in new[] { "1.0.0", "1.5.0" })
{
    var folder = ToolCacheLayout.GetToolPath("sample-tool", version, arch);
    Directory.CreateDirectory(folder);
    File.WriteAllText(Path.Combine(folder, "tool.bin"), $"contents-{version}");
    File.WriteAllText(folder + ".complete", string.Empty);
}

Console.WriteLine($"Find exact: {toolCache.Find("sample-tool", "1.0.0")}");
Console.WriteLine($"Find range '1.x' -> {toolCache.Find("sample-tool", "1.x")}");
Console.WriteLine($"Find missing -> '{toolCache.Find("sample-tool", "9.9.9")}'");

Console.WriteLine();
Console.WriteLine("FindAllVersions('sample-tool'):");
foreach (var v in toolCache.FindAllVersions("sample-tool"))
{
    Console.WriteLine($"  {v}");
}
