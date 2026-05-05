// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkitSharp.IO.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGitHubActionsIORegistersIOperations()
    {
        var services = new ServiceCollection();

        services.AddGitHubActionsIO();

        using var provider = services.BuildServiceProvider();
        var operations = provider.GetRequiredService<IOperations>();
        Assert.NotNull(operations);
        Assert.IsType<Operations>(operations);
    }

    [Fact]
    public void AddGitHubActionsIORegistersIOperationsAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddGitHubActionsIO();

        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IOperations>();
        var second = provider.GetRequiredService<IOperations>();
        Assert.Same(first, second);
    }

    [Fact]
    public void AddGitHubActionsIOReturnsSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddGitHubActionsIO();

        Assert.Same(services, result);
    }
}
