// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .AddCacheServices()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();
var cache = provider.GetRequiredService<ICacheClient>();

if (!cache.IsFeatureAvailable())
{
    core.WriteWarning(
        "Cache service is not available — set ACTIONS_CACHE_SERVICE_V2=1 and ACTIONS_RESULTS_URL/ACTIONS_RUNTIME_TOKEN to exercise this sample on a hosted runner.");
    return;
}

try
{
    var key = args is [var primaryKey, ..] ? primaryKey : $"sample-{DateTimeOffset.UtcNow:yyyyMMdd}";

    // Stage a small "build output" tree under a temp root, then save+restore
    // it. This mirrors the upstream `@actions/cache` README's "save / restore
    // a cache" examples: pick a key, list the paths to cache, hand them to
    // the client.
    var workspace = Path.Combine(Path.GetTempPath(), $"cache-sample-{Guid.NewGuid():N}");
    Directory.CreateDirectory(workspace);
    var binDir = Path.Combine(workspace, "bin");
    Directory.CreateDirectory(binDir);
    await File.WriteAllTextAsync(Path.Combine(binDir, "compiled.txt"), "build output\n");
    await File.WriteAllTextAsync(Path.Combine(binDir, "manifest.json"), "{\"sample\":true}\n");

    Environment.SetEnvironmentVariable("GITHUB_WORKSPACE", workspace);

    string[] paths = ["bin"];

    // Try restore first.
    var restored = await cache.RestoreCacheAsync(
        paths,
        key,
        restoreKeys: ["sample-"]);

    if (restored is not null)
    {
        core.WriteInfo($"Cache hit on key {restored} — skipping rebuild.");
    }
    else
    {
        core.WriteInfo($"Cache miss for {key} — running fake build.");
        // Save the build output under the primary key.
        var saved = await cache.SaveCacheAsync(paths, key);
        if (saved is not null)
        {
            core.WriteInfo(
                $"Saved cache key={saved.Key} version={saved.Version} size={saved.Size} bytes.");
        }
    }
}
catch (Exception ex)
{
    core.SetFailed(ex.ToString());
}
