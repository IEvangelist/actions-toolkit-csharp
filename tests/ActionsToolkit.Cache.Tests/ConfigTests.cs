// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Tests;

/// <summary>
/// Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/__tests__/config.test.ts">
/// <c>__tests__/config.test.ts</c></see>. Exercises the <c>isGhes()</c>
/// helper port at <see cref="CacheUtils.IsGhes"/> against the four
/// canonical <c>GITHUB_SERVER_URL</c> shapes.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class ConfigTests
{
    [Fact(DisplayName = "isGhes returns false for github.com")]
    public void IsGhes_False_ForGitHubCom()
    {
        using var _ = new EnvironmentScope("GITHUB_SERVER_URL", "https://github.com");

        Assert.False(CacheUtils.IsGhes());
    }

    [Fact(DisplayName = "isGhes returns false for ghe.com")]
    public void IsGhes_False_ForGheCom()
    {
        using var _ = new EnvironmentScope("GITHUB_SERVER_URL", "https://somedomain.ghe.com");

        Assert.False(CacheUtils.IsGhes());
    }

    [Fact(DisplayName = "isGhes returns true for enterprise URL")]
    public void IsGhes_True_ForEnterpriseUrl()
    {
        using var _ = new EnvironmentScope("GITHUB_SERVER_URL", "https://my-enterprise.github.com");

        Assert.True(CacheUtils.IsGhes());
    }

    [Fact(DisplayName = "isGhes returns false for ghe.localhost")]
    public void IsGhes_False_ForGheLocalhost()
    {
        using var _ = new EnvironmentScope("GITHUB_SERVER_URL", "https://my.domain.ghe.localhost");

        Assert.False(CacheUtils.IsGhes());
    }
}
