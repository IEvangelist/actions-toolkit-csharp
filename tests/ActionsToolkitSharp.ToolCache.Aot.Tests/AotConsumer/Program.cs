// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkitSharp.HttpClient.Extensions;
using ActionsToolkitSharp.ToolCache;
using ActionsToolkitSharp.ToolCache.Extensions;
using ActionsToolkitSharp.ToolCache.Layout;
using ActionsToolkitSharp.ToolCache.Manifest;
using ActionsToolkitSharp.ToolCache.Semver;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace ActionsToolkitSharp.ToolCache.AotConsumer;

/// <summary>
/// Native AOT dispatcher that exercises the public surface of
/// <see cref="ActionsToolkitSharp.ToolCache"/> so that the trimmer roots every
/// API and validates the source-gen JSON path.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: ats-toolcache-aot-consumer <case> [sandbox-path]");
        }

        try
        {
            var sandbox = args.Length >= 2
                ? args[1]
                : Path.Combine(Path.GetTempPath(), "ats-tc-aot-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sandbox);

            var cacheRoot = Path.Combine(sandbox, "cache");
            var tempRoot = Path.Combine(sandbox, "temp");
            Directory.CreateDirectory(cacheRoot);
            Directory.CreateDirectory(tempRoot);
            Environment.SetEnvironmentVariable(ToolCacheLayout.CacheDirectoryEnvVar, cacheRoot);
            Environment.SetEnvironmentVariable(ToolCacheLayout.TempDirectoryEnvVar, tempRoot);

            return args[0] switch
            {
                "instantiate-service" => RunInstantiateService(),
                "evaluate-versions" => RunEvaluateVersions(),
                "find" => RunFind(cacheRoot),
                "cache-dir-layout" => RunCacheDirLayout(sandbox),
                "manifest-deserialize" => RunManifestDeserialize(),
                _ => Fail($"Unknown case: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            return Fail($"{ex.GetType().FullName}: {ex.Message}");
        }
    }

    private static int RunInstantiateService()
    {
        using var sp = new ServiceCollection()
            .AddGitHubActionsToolCache()
            .BuildServiceProvider();
        var svc = sp.GetRequiredService<IToolCacheService>();
        return svc is null ? Fail("IToolCacheService resolution returned null.") : Ok("instantiate-service", svc.GetType().FullName ?? "<unknown>");
    }

    private static int RunEvaluateVersions()
    {
        using var sp = new ServiceCollection()
            .AddGitHubActionsToolCache()
            .BuildServiceProvider();
        var svc = sp.GetRequiredService<IToolCacheService>();

        string[] versions = ["1.0.0", "1.5.0", "2.0.0", "2.5.0-rc.1"];
        var match = svc.EvaluateVersions(versions, "1.x");
        if (match != "1.5.0")
        {
            return Fail($"evaluate-versions expected 1.5.0, got '{match}'");
        }

        return Ok("evaluate-versions", match);
    }

    private static int RunFind(string cacheRoot)
    {
        using var sp = new ServiceCollection()
            .AddGitHubActionsToolCache()
            .BuildServiceProvider();
        var svc = sp.GetRequiredService<IToolCacheService>();

        var arch = ToolCacheLayout.GetDefaultArch();
        var folder = Path.Combine(cacheRoot, "node", "16.0.0", arch);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "node.exe"), "stub");
        File.WriteAllText(folder + ".complete", string.Empty);

        var resolved = svc.Find("node", "16.0.0", arch);
        if (resolved != folder)
        {
            return Fail($"find expected '{folder}' got '{resolved}'");
        }

        var ranged = svc.Find("node", "16.x", arch);
        if (ranged != folder)
        {
            return Fail($"find with range expected '{folder}' got '{ranged}'");
        }

        return Ok("find", resolved);
    }

    private static int RunCacheDirLayout(string sandbox)
    {
        var arch = ToolCacheLayout.GetDefaultArch();
        var plat = ToolCacheLayout.GetDefaultPlatform();
        var path = ToolCacheLayout.GetToolPath("node", "v18.1.0", arch);
        var marker = ToolCacheLayout.GetMarkerPath(path);

        if (!path.EndsWith(Path.Combine("node", "18.1.0", arch), StringComparison.Ordinal))
        {
            return Fail($"layout path mismatch: {path}");
        }

        if (!marker.EndsWith(".complete", StringComparison.Ordinal))
        {
            return Fail($"layout marker mismatch: {marker}");
        }

        if (string.IsNullOrEmpty(plat))
        {
            return Fail("layout platform empty");
        }

        if (!NpmVersion.TryParse("v1.2.3", out var v) || v.ToString() != "1.2.3")
        {
            return Fail("NpmVersion.TryParse round-trip mismatch");
        }

        if (!NpmVersionRange.Satisfies("1.2.3", "^1.0.0"))
        {
            return Fail("NpmVersionRange.Satisfies returned false for ^1.0.0");
        }

        return Ok("cache-dir-layout", $"{path} | {plat}");
    }

    private static int RunManifestDeserialize()
    {
        var json = """
        [
          {
            "version": "3.0.0",
            "stable": true,
            "release_url": "https://example.test/3",
            "files": [
              { "filename": "tool-3-linux-x64.tar.gz", "platform": "linux", "arch": "x64", "download_url": "https://example.test/3-linux" }
            ]
          },
          {
            "version": "2.0.0",
            "stable": false,
            "release_url": "https://example.test/2",
            "files": []
          }
        ]
        """;

        var releases = JsonSerializer.Deserialize(json, ManifestJsonContext.Default.ListToolRelease);
        if (releases is null || releases.Count != 2)
        {
            return Fail($"manifest-deserialize wrong count: {releases?.Count}");
        }

        if (releases[0].Version != "3.0.0" || !releases[0].Stable || releases[0].Files.Count != 1)
        {
            return Fail("manifest-deserialize first release mismatch");
        }

        if (releases[1].Version != "2.0.0" || releases[1].Stable)
        {
            return Fail("manifest-deserialize second release mismatch");
        }

        return Ok("manifest-deserialize", $"{releases.Count} releases");
    }

    private static int Ok(string @case, string detail = "")
    {
        Console.WriteLine($"[OK] {@case}{(detail.Length > 0 ? $" {detail}" : string.Empty)}");
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine($"[FAIL] {message}");
        return 1;
    }
}
