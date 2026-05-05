// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Exec.Tests;

/// <summary>
/// Validates DI registration via <c>AddGitHubActionsExec</c> — mirrors the
/// <c>ServiceCollectionExtensionsTests</c> shape used by sibling packages.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "AddGitHubActionsExec registers IExecService")]
    public void AddGitHubActionsExecRegistersIExecService()
    {
        using var provider = new ServiceCollection()
            .AddGitHubActionsExec()
            .BuildServiceProvider();

        var exec = provider.GetService<IExecService>();
        Assert.NotNull(exec);
    }

    [Fact(DisplayName = "AddGitHubActionsExec returns the same IServiceCollection for chaining")]
    public void AddGitHubActionsExecReturnsTheSameIServiceCollectionForChaining()
    {
        var services = new ServiceCollection();

        var returned = services.AddGitHubActionsExec();

        Assert.Same(services, returned);
    }

    [Fact(DisplayName = "AddGitHubActionsExec resolves IExecService with default lifetime")]
    public void AddGitHubActionsExecResolvesIExecServiceWithDefaultLifetime()
    {
        using var provider = new ServiceCollection()
            .AddGitHubActionsExec()
            .BuildServiceProvider();

        var first = provider.GetRequiredService<IExecService>();
        var second = provider.GetRequiredService<IExecService>();

        // Transient lifetime — each resolution returns a new instance.
        Assert.NotSame(first, second);
    }
}
