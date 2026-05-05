// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// evaluate-versions.cs
//
// Mirrors the upstream `@actions/tool-cache` README "evaluateVersions" snippet:
// https://github.com/actions/toolkit/tree/main/packages/tool-cache#evaluateversions
//
// Picks the highest version from a list that satisfies a node-semver range.
// Useful for resolving a manifest into a concrete version before downloading.
//
// Run with:
//   dotnet run evaluate-versions.cs

#:package ActionsToolkit.ToolCache@*
#:package ActionsToolkit.HttpClient@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.ToolCache;
using ActionsToolkit.ToolCache.Extensions;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsToolCache()
    .BuildServiceProvider();

var toolCache = provider.GetRequiredService<IToolCacheService>();

string[] versions = ["1.0.0", "1.2.3", "1.5.0", "2.0.0", "2.5.0-rc.1", "3.0.0"];

string[] specs = ["1.x", "^1.0.0", "~1.2.0", ">=2.0.0", "1.x || 2.x", "*"];

foreach (var spec in specs)
{
    var match = toolCache.EvaluateVersions(versions, spec);
    Console.WriteLine($"EvaluateVersions(spec='{spec}') -> {match ?? "<none>"}");
}
