// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Verifies <see cref="ServiceCollectionExtensions.AddGitHubActionsArtifact"/>
/// wires up every public service required by <see cref="IArtifactClient"/>.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "AddGitHubActionsArtifact registers IArtifactClient")]
    public void AddRegistersClient()
    {
        using var provider = new ServiceCollection()
            .AddGitHubActionsArtifact()
            .BuildServiceProvider();

        var client = provider.GetService<IArtifactClient>();

        Assert.NotNull(client);
        Assert.IsType<DefaultArtifactClient>(client);
    }

    [Fact(DisplayName = "AddGitHubActionsArtifact registers IPublicArtifactsApi and IArtifactService")]
    public void AddRegistersInternalServices()
    {
        using var provider = new ServiceCollection()
            .AddGitHubActionsArtifact()
            .BuildServiceProvider();

        Assert.NotNull(provider.GetService<IPublicArtifactsApi>());
        Assert.NotNull(provider.GetService<IArtifactService>());
        Assert.NotNull(provider.GetService<IBackendIdsProvider>());
    }
}
