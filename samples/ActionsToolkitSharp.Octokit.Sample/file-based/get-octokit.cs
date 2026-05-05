// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// get-octokit.cs
//
// Mirrors the upstream `@actions/github` README example:
// https://github.com/actions/toolkit/tree/main/packages/github
//
// Reads a GitHub token from the `myToken` action input, registers a
// hydrated `GitHubClient` (from the `GitHub.Octokit.SDK` package) with
// the DI container, and uses it to fetch a pull request from a public
// repository.
//
// Run with:
//   INPUT_MYTOKEN=$GITHUB_TOKEN dotnet run get-octokit.cs

#:package ActionsToolkitSharp.Core@*
#:package ActionsToolkitSharp.Octokit@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Core.Extensions;
using ActionsToolkitSharp.Core.Services;
using ActionsToolkitSharp.Octokit.Extensions;
using GitHub;
using Microsoft.Extensions.DependencyInjection;

// Stage 1: read the token using the core service.
using var coreProvider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = coreProvider.GetRequiredService<ICoreService>();
var myToken = core.GetInput("myToken");

if (string.IsNullOrWhiteSpace(myToken))
{
    core.SetFailed("Required input 'myToken' was not provided.");
    return;
}

// Stage 2: register the hydrated GitHub client with the token.
using var provider = new ServiceCollection()
    .AddGitHubClientServices(myToken)
    .BuildServiceProvider();

var client = provider.GetRequiredService<GitHubClient>();

// Call the GitHub REST API: GET /repos/octokit/rest.js/pulls/123
var pullRequest = await client.Repos["octokit"]["rest.js"].Pulls[123].GetAsync();

core.WriteInfo($"Pull request title: {pullRequest?.Title}");
core.WriteInfo($"Pull request state: {pullRequest?.State}");
