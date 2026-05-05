// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.ToolCache.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGitHubActionsToolCacheRegistersIToolCacheService()
    {
        var services = new ServiceCollection();
        services.AddGitHubActionsToolCache();

        using var provider = services.BuildServiceProvider();
        var toolCache = provider.GetRequiredService<IToolCacheService>();
        Assert.NotNull(toolCache);
    }

    [Fact]
    public void AddGitHubActionsToolCacheRegistersIRetryHelper()
    {
        var services = new ServiceCollection();
        services.AddGitHubActionsToolCache();

        using var provider = services.BuildServiceProvider();
        var retry = provider.GetRequiredService<IRetryHelper>();
        Assert.NotNull(retry);
    }

    [Fact]
    public void AddGitHubActionsToolCacheRegistersIToolCacheServiceAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddGitHubActionsToolCache();

        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IToolCacheService>();
        var second = provider.GetRequiredService<IToolCacheService>();
        Assert.Same(first, second);
    }

    [Fact]
    public void AddGitHubActionsToolCacheReturnsSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();
        var result = services.AddGitHubActionsToolCache();

        Assert.Same(services, result);
    }
}
