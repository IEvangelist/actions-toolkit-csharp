// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Tests;

public sealed class ToolCacheLayoutTests : IDisposable
{
    private readonly TempCacheFixture _fixture = new();

    public void Dispose() => _fixture.Dispose();

    [Fact(DisplayName = "GetCacheDirectory returns RUNNER_TOOL_CACHE")]
    public void GetCacheDirectoryReturnsEnvVar()
    {
        Assert.Equal(_fixture.CacheRoot, ToolCacheLayout.GetCacheDirectory());
    }

    [Fact(DisplayName = "GetTempDirectory returns RUNNER_TEMP")]
    public void GetTempDirectoryReturnsEnvVar()
    {
        Assert.Equal(_fixture.TempRoot, ToolCacheLayout.GetTempDirectory());
    }

    [Fact(DisplayName = "GetToolPath composes tool/version/arch")]
    public void GetToolPathComposesParts()
    {
        var p = ToolCacheLayout.GetToolPath("node", "16.0.0", "x64");
        Assert.Equal(Path.Combine(_fixture.CacheRoot, "node", "16.0.0", "x64"), p);
    }

    [Fact(DisplayName = "GetMarkerPath appends .complete")]
    public void GetMarkerPathAppendsComplete()
    {
        var folder = Path.Combine(_fixture.CacheRoot, "node", "16.0.0", "x64");
        Assert.Equal(folder + ".complete", ToolCacheLayout.GetMarkerPath(folder));
    }

    [Fact(DisplayName = "GetCacheDirectory throws when env var missing")]
    public void GetCacheDirectoryThrowsWhenMissing()
    {
        Environment.SetEnvironmentVariable(ToolCacheLayout.CacheDirectoryEnvVar, null);
        try
        {
            Assert.Throws<InvalidOperationException>(() => ToolCacheLayout.GetCacheDirectory());
        }
        finally
        {
            Environment.SetEnvironmentVariable(ToolCacheLayout.CacheDirectoryEnvVar, _fixture.CacheRoot);
        }
    }

    [Fact(DisplayName = "GetDefaultArch returns recognized identifier")]
    public void GetDefaultArchReturnsRecognizedIdentifier()
    {
        var arch = ToolCacheLayout.GetDefaultArch();
        Assert.False(string.IsNullOrEmpty(arch));
    }

    [Fact(DisplayName = "GetDefaultPlatform returns recognized identifier")]
    public void GetDefaultPlatformReturnsRecognizedIdentifier()
    {
        var plat = ToolCacheLayout.GetDefaultPlatform();
        Assert.False(string.IsNullOrEmpty(plat));
    }
}
