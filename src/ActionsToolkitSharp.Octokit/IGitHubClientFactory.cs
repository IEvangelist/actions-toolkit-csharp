// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Octokit;

/// <summary>
/// Factory abstraction over <see cref="GitHubClientFactory.Create(string)"/> so
/// that consumer packages (e.g. <c>ActionsToolkitSharp.Attest</c>) can resolve a
/// per-call <see cref="GitHubClient"/> from DI without taking a hard dependency
/// on the static factory.
/// </summary>
public interface IGitHubClientFactory
{
    /// <summary>
    /// Creates a new <see cref="GitHubClient"/> hydrated with the supplied
    /// <paramref name="token"/>.
    /// </summary>
    /// <param name="token">A GitHub token (e.g.
    /// <c>${{ secrets.GITHUB_TOKEN }}</c>) used for authentication.</param>
    /// <returns>A new <see cref="GitHubClient"/>.</returns>
    GitHubClient Create(string token);
}

/// <summary>
/// Default <see cref="IGitHubClientFactory"/> that delegates to the static
/// <see cref="GitHubClientFactory.Create(string)"/> helper. Registered by
/// <c>AddGitHubClientServices</c>.
/// </summary>
public sealed class DefaultGitHubClientFactory : IGitHubClientFactory
{
    /// <inheritdoc />
    public GitHubClient Create(string token) => GitHubClientFactory.Create(token);
}
