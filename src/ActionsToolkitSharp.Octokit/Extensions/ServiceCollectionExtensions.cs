// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Octokit.Extensions;

/// <summary>
/// Represents a collection of extension methods on <see cref="IServiceCollection"/>
/// for adding GitHub services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Octokit services to the <see cref="IServiceCollection"/>.
    /// <list type="bullet">
    /// <item>A <see cref="GitHubClient"/> instance initialized with the token.</item>
    /// <item>A <see cref="Context"/> as materialized from the workflows environment.</item>
    /// </list>
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="gitHubToken">The GitHub token used to create the <see cref="GitHubClient"/>.
    /// Commonly assigned from <c>${{ secrets.GITHUB_TOKEN }}</c>
    /// </param>
    /// <returns>The same service collection, but with added services.</returns>
    public static IServiceCollection AddGitHubClientServices(
        this IServiceCollection services,
        string gitHubToken)
    {
        services.AddSingleton(GitHubClientFactory.Create(gitHubToken));

        services.AddSingleton(Context.Current);

        return services;
    }
}
