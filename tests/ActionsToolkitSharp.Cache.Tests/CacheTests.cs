// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Unit tests for the public surface of <see cref="ICacheClient"/>. Mirrors
/// upstream <c>__tests__/cache.test.ts</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class CacheTests
{
    [Fact(DisplayName = "isFeatureAvailable returns true when ACTIONS_RESULTS_URL is set on V2")]
    public void IsFeatureAvailable_True_OnV2_WithResultsUrl()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("ACTIONS_CACHE_URL", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");

        Assert.True(CacheUtils.IsFeatureAvailable());
    }

    [Fact(DisplayName = "isFeatureAvailable returns false when ACTIONS_RESULTS_URL is missing on V2")]
    public void IsFeatureAvailable_False_OnV2_WithoutResultsUrl()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", null)
            .Set("ACTIONS_CACHE_URL", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");

        Assert.False(CacheUtils.IsFeatureAvailable());
    }

    [Fact(DisplayName = "isFeatureAvailable returns true when ACTIONS_CACHE_URL is set on V1")]
    public void IsFeatureAvailable_True_OnV1_WithCacheUrl()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", null)
            .Set("ACTIONS_CACHE_URL", "https://cache.example.com/")
            .Set("ACTIONS_RESULTS_URL", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");

        Assert.True(CacheUtils.IsFeatureAvailable());
    }

    [Fact(DisplayName = "isFeatureAvailable returns false when ACTIONS_CACHE_URL is missing on V1")]
    public void IsFeatureAvailable_False_OnV1_WithoutCacheUrl()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", null)
            .Set("ACTIONS_CACHE_URL", null)
            .Set("ACTIONS_RESULTS_URL", null)
            .Set("GITHUB_SERVER_URL", "https://github.com");

        Assert.False(CacheUtils.IsFeatureAvailable());
    }

    [Fact(DisplayName = "ICacheClient.IsFeatureAvailable() round-trips CacheUtils")]
    public void Client_IsFeatureAvailable_RoundTrips()
    {
        using var _ = new EnvironmentScopeBag()
            .Set("ACTIONS_CACHE_SERVICE_V2", "1")
            .Set("ACTIONS_RESULTS_URL", "https://results.example.com/")
            .Set("GITHUB_SERVER_URL", "https://github.com");

        using var provider = new ServiceCollection()
            .AddCacheServices()
            .BuildServiceProvider();

        var client = provider.GetRequiredService<ICacheClient>();
        Assert.True(client.IsFeatureAvailable());
    }
}
