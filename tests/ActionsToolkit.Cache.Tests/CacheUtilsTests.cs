// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Tests;

/// <summary>
/// Unit tests for the env-driven cache config helpers. Mirrors upstream
/// <c>__tests__/config.test.ts</c> behavior.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class CacheUtilsTests
{
    [Fact(DisplayName = "getCacheServiceVersion returns v2 when ACTIONS_CACHE_SERVICE_V2 is set")]
    public void GetCacheServiceVersion_V2_WhenEnabled()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("GITHUB_SERVER_URL", "https://github.com");

        Assert.Equal(CacheServiceVersion.V2, CacheUtils.GetCacheServiceVersion());
    }

    [Fact(DisplayName = "getCacheServiceVersion returns v1 when ACTIONS_CACHE_SERVICE_V2 is unset")]
    public void GetCacheServiceVersion_V1_WhenDisabled()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");

        Assert.Equal(CacheServiceVersion.V1, CacheUtils.GetCacheServiceVersion());
    }

    [Fact(DisplayName = "getCacheServiceVersion returns v1 when running on GHES even with V2 enabled")]
    public void GetCacheServiceVersion_V1_OnGhes()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("GITHUB_SERVER_URL", "https://my-ghes.example.com");

        Assert.Equal(CacheServiceVersion.V1, CacheUtils.GetCacheServiceVersion());
    }

    [Fact(DisplayName = "getCacheServiceURL prefers ACTIONS_RESULTS_URL on V2")]
    public void GetCacheServiceUrl_V2_PrefersResultsUrl()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_CACHE_URL", "https://cache.example.com/")
            .Set("GITHUB_SERVER_URL", "https://github.com");

        Assert.Equal("https://results.example.com/", CacheUtils.GetCacheServiceUrl());
    }

    [Fact(DisplayName = "getCacheServiceURL prefers ACTIONS_CACHE_URL on V1")]
    public void GetCacheServiceUrl_V1_PrefersCacheUrl()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", null)
            .Set("ACTIONS_CACHE_URL", "https://cache.example.com/")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("GITHUB_SERVER_URL", "https://github.com");

        Assert.Equal("https://cache.example.com/", CacheUtils.GetCacheServiceUrl());
    }

    [Fact(DisplayName = "getRuntimeToken throws when ACTIONS_RUNTIME_TOKEN is missing")]
    public void GetRuntimeToken_Throws_WhenMissing()
    {
        using var _ = new EnvironmentScope("ACTIONS_RUNTIME_TOKEN", null);

        Assert.Throws<CacheServiceUnavailableException>(() => CacheUtils.GetRuntimeToken());
    }

    [Fact(DisplayName = "getRuntimeToken returns the env value when set")]
    public void GetRuntimeToken_Returns_WhenSet()
    {
        using var _ = new EnvironmentScope("ACTIONS_RUNTIME_TOKEN", "ghs_xxx");

        Assert.Equal("ghs_xxx", CacheUtils.GetRuntimeToken());
    }

    [Fact(DisplayName = "checkPaths throws when paths is null")]
    public void CheckPaths_Throws_WhenNull() =>
        Assert.Throws<CacheValidationException>(() => CacheUtils.CheckPaths(null));

    [Fact(DisplayName = "checkPaths throws when paths is empty")]
    public void CheckPaths_Throws_WhenEmpty() =>
        Assert.Throws<CacheValidationException>(() => CacheUtils.CheckPaths([]));

    [Fact(DisplayName = "checkPaths succeeds for non-empty list")]
    public void CheckPaths_Ok() => CacheUtils.CheckPaths(["bin"]);

    [Fact(DisplayName = "checkKey throws when key exceeds 512 characters")]
    public void CheckKey_Throws_WhenTooLong()
    {
        var key = new string('a', 513);
        var ex = Assert.Throws<CacheValidationException>(() => CacheUtils.CheckKey(key));
        Assert.Contains("512", ex.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "checkKey throws when key contains commas")]
    public void CheckKey_Throws_WhenContainsCommas()
    {
        var ex = Assert.Throws<CacheValidationException>(() => CacheUtils.CheckKey("a,b"));
        Assert.Contains("commas", ex.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "checkKey allows a normal key")]
    public void CheckKey_Ok() => CacheUtils.CheckKey("build-linux-abc123");

    [Fact(DisplayName = "getCacheVersion is deterministic for identical inputs")]
    public void GetCacheVersion_Deterministic()
    {
        var a = CacheUtils.GetCacheVersion(["bin", "obj"], CompressionMethod.ZstdWithoutLong);
        var b = CacheUtils.GetCacheVersion(["bin", "obj"], CompressionMethod.ZstdWithoutLong);
        Assert.Equal(a, b);
        Assert.Equal(64, a.Length);
    }

    [Fact(DisplayName = "getCacheVersion changes when paths change")]
    public void GetCacheVersion_ChangesOnPaths()
    {
        var a = CacheUtils.GetCacheVersion(["bin"], CompressionMethod.ZstdWithoutLong);
        var b = CacheUtils.GetCacheVersion(["obj"], CompressionMethod.ZstdWithoutLong);
        Assert.NotEqual(a, b);
    }

    [Fact(DisplayName = "getCacheVersion changes when compression method changes")]
    public void GetCacheVersion_ChangesOnCompression()
    {
        var a = CacheUtils.GetCacheVersion(["bin"], CompressionMethod.ZstdWithoutLong);
        var b = CacheUtils.GetCacheVersion(["bin"], CompressionMethod.Gzip);
        Assert.NotEqual(a, b);
    }

    [Fact(DisplayName = "getCacheVersion mixes 'windows-only' on Windows when enableCrossOsArchive is false")]
    public void GetCacheVersion_WindowsMarker()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // marker is only added on Windows
        }

        var winOnly = CacheUtils.GetCacheVersion(
            ["bin"], CompressionMethod.ZstdWithoutLong, enableCrossOsArchive: false);
        var crossOs = CacheUtils.GetCacheVersion(
            ["bin"], CompressionMethod.ZstdWithoutLong, enableCrossOsArchive: true);

        Assert.NotEqual(winOnly, crossOs);
    }

    [Fact(DisplayName = "getCacheFileName returns .tzst for zstd")]
    public void GetCacheFileName_Zstd() =>
        Assert.Equal("cache.tzst", CacheUtils.GetCacheFileName(CompressionMethod.ZstdWithoutLong));

    [Fact(DisplayName = "getCacheFileName returns .tgz for gzip")]
    public void GetCacheFileName_Gzip() =>
        Assert.Equal("cache.tgz", CacheUtils.GetCacheFileName(CompressionMethod.Gzip));
}
