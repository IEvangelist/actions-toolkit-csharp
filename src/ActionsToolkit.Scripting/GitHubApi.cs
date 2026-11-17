// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Octokit;

namespace ActionsToolkit.Scripting;

/// <summary>GitHub runner context and explicitly authenticated API clients.</summary>
public static class GitHubApi
{
    /// <summary>Gets the existing process-local runner context; unrelated modules do not initialize it.</summary>
    public static Context Context => Context.Current;

    /// <summary>Creates the existing GitHub SDK client with an explicit, nonempty token.</summary>
    /// <remarks>The returned SDK client is not IDisposable. This method adds no provider or hidden owned lifetime.</remarks>
    public static global::GitHub.GitHubClient CreateClient(string token) => GitHubClientFactory.Create(token);
}
