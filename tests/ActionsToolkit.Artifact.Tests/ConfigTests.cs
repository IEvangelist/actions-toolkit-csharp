// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/config.test.ts</c>: covers
/// <see cref="ArtifactConfig.IsGhes"/> and
/// <see cref="ArtifactConfig.GetGitHubWorkspaceDirectory"/>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class ConfigTests
{
    [Fact(DisplayName = "isGHES returns false for github.com")]
    public void IsGhesReturnsFalseForGitHubCom()
    {
        using var _ = new EnvironmentScope("GITHUB_SERVER_URL", "https://github.com");

        Assert.False(ArtifactConfig.IsGhes());
    }

    [Fact(DisplayName = "isGHES returns false for *.ghe.com")]
    public void IsGhesReturnsFalseForGhecHost()
    {
        using var _ = new EnvironmentScope("GITHUB_SERVER_URL", "https://my-tenant.ghe.com");

        Assert.False(ArtifactConfig.IsGhes());
    }

    [Fact(DisplayName = "isGHES returns false for *.localhost")]
    public void IsGhesReturnsFalseForLocalhost()
    {
        using var _ = new EnvironmentScope("GITHUB_SERVER_URL", "https://my-tenant.localhost");

        Assert.False(ArtifactConfig.IsGhes());
    }

    [Fact(DisplayName = "isGHES returns true for an enterprise server hostname")]
    public void IsGhesReturnsTrueForEnterpriseServer()
    {
        using var _ = new EnvironmentScope("GITHUB_SERVER_URL", "https://my-ghes.example.com");

        Assert.True(ArtifactConfig.IsGhes());
    }

    [Fact(DisplayName = "isGHES defaults to false when GITHUB_SERVER_URL is unset")]
    public void IsGhesDefaultsToFalseWhenUnset()
    {
        using var _ = new EnvironmentScope("GITHUB_SERVER_URL", value: null);

        Assert.False(ArtifactConfig.IsGhes());
    }

    [Fact(DisplayName = "getGitHubWorkspaceDir returns GITHUB_WORKSPACE when set")]
    public void GetWorkspaceReturnsEnvWhenSet()
    {
        using var _ = new EnvironmentScope("GITHUB_WORKSPACE", "/tmp/ws");

        Assert.Equal("/tmp/ws", ArtifactConfig.GetGitHubWorkspaceDirectory());
    }

    [Fact(DisplayName = "getGitHubWorkspaceDir falls back to current directory when unset")]
    public void GetWorkspaceFallsBackToCurrentDirectory()
    {
        using var _ = new EnvironmentScope("GITHUB_WORKSPACE", value: null);

        Assert.Equal(Environment.CurrentDirectory, ArtifactConfig.GetGitHubWorkspaceDirectory());
    }
}
