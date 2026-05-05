// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Tests;

/// <summary>
/// Per-test isolated <c>RUNNER_TOOL_CACHE</c> + <c>RUNNER_TEMP</c> sandbox.
/// </summary>
internal sealed class TempCacheFixture : IDisposable
{
    private readonly string? _previousCache;
    private readonly string? _previousTemp;

    public TempCacheFixture()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        CacheRoot = Path.Combine(root, "cache");
        TempRoot = Path.Combine(root, "temp");
        Directory.CreateDirectory(CacheRoot);
        Directory.CreateDirectory(TempRoot);

        _previousCache = Environment.GetEnvironmentVariable(ToolCacheLayout.CacheDirectoryEnvVar);
        _previousTemp = Environment.GetEnvironmentVariable(ToolCacheLayout.TempDirectoryEnvVar);
        Environment.SetEnvironmentVariable(ToolCacheLayout.CacheDirectoryEnvVar, CacheRoot);
        Environment.SetEnvironmentVariable(ToolCacheLayout.TempDirectoryEnvVar, TempRoot);
    }

    public string CacheRoot { get; }
    public string TempRoot { get; }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(ToolCacheLayout.CacheDirectoryEnvVar, _previousCache);
        Environment.SetEnvironmentVariable(ToolCacheLayout.TempDirectoryEnvVar, _previousTemp);
        try
        {
            var parent = Directory.GetParent(CacheRoot)?.FullName;
            if (parent is not null && Directory.Exists(parent))
            {
                Directory.Delete(parent, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
