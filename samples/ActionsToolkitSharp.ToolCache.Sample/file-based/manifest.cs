// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// manifest.cs
//
// Mirrors the upstream `@actions/tool-cache` README "Manifest" snippet:
// https://github.com/actions/toolkit/tree/main/packages/tool-cache#manifest
//
// Demonstrates `findFromManifest` against an in-memory manifest. To run
// against a real GitHub repo's `versions-manifest.json`, swap the manifest
// for the result of `toolCache.GetManifestFromRepoAsync(owner, repo, token)`.
//
// Run with:
//   dotnet run manifest.cs

#:package ActionsToolkitSharp.ToolCache@*
#:package ActionsToolkitSharp.HttpClient@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.ToolCache;
using ActionsToolkitSharp.ToolCache.Extensions;
using ActionsToolkitSharp.ToolCache.Layout;
using ActionsToolkitSharp.ToolCache.Manifest;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsToolCache()
    .BuildServiceProvider();

var toolCache = provider.GetRequiredService<IToolCacheService>();

// Build an in-memory manifest. In production code, prefer
// `await toolCache.GetManifestFromRepoAsync("actions", "node-versions", token)`.
var plat = ToolCacheLayout.GetDefaultPlatform();
var arch = ToolCacheLayout.GetDefaultArch();

IReadOnlyList<IToolRelease> manifest =
[
    new InMemoryRelease("3.0.0", true,
    [
        new InMemoryFile("tool-3.0.0.tar.gz", plat, arch, "https://example.test/3"),
    ]),
    new InMemoryRelease("2.0.0", true,
    [
        new InMemoryFile("tool-2.0.0.tar.gz", plat, arch, "https://example.test/2"),
    ]),
    new InMemoryRelease("1.5.0-rc.1", false,
    [
        new InMemoryFile("tool-1.5.0-rc.1.tar.gz", plat, arch, "https://example.test/1.5"),
    ]),
];

var match = await toolCache.FindFromManifestAsync("2.x || 3.x", stable: true, manifest);

if (match is null)
{
    Console.WriteLine("No matching release.");
    return;
}

Console.WriteLine($"Best match: {match.Version} (stable={match.Stable})");
foreach (var f in match.Files)
{
    Console.WriteLine($"  {f.Filename}  {f.Platform}/{f.Arch} -> {f.DownloadUrl}");
}

internal sealed record InMemoryRelease(
    string Version,
    bool Stable,
    IReadOnlyList<IToolReleaseFile> Files,
    string ReleaseUrl = "") : IToolRelease;

internal sealed record InMemoryFile(
    string Filename,
    string Platform,
    string Arch,
    string DownloadUrl,
    string? PlatformVersion = null) : IToolReleaseFile;
