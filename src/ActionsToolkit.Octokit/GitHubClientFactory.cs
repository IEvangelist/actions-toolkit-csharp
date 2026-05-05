// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Octokit;

/// <summary>
/// Represents a factory for creating <see cref="GitHubClient"/> instances.
/// </summary>
public static class GitHubClientFactory
{
    /// <summary>
    /// Creates a new <see cref="GitHubClient"/> from the given <paramref name="token"/>.
    /// </summary>
    /// <param name="token">The token used to initialize the client.</param>
    /// <returns>A new <see cref="GitHubClient"/> instance.</returns>
    public static GitHubClient Create(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var tokenProvider = new TokenProvider(token);

        var request = RequestAdapter.Create(
            new TokenAuthProvider(tokenProvider));

        return new GitHubClient(request);
    }
}
