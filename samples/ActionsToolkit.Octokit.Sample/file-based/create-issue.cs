// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// create-issue.cs
//
// Mirrors the upstream `@actions/github` README "context" snippet that
// creates an issue using the current repository context:
// https://github.com/actions/toolkit/tree/main/packages/github
//
// In the upstream Node example:
//
//   const newIssue = await octokit.rest.issues.create({
//     ...context.repo,
//     title: 'New issue!',
//     body: 'Hello Universe!',
//   });
//
// In ActionsToolkit the equivalent is to combine `Context.Current.Repo`
// with the Kiota-generated `client.Repos[owner][repo].Issues.PostAsync(...)`.
//
// IMPORTANT: this script will actually create an issue if you supply a
// real token + repository. It is shown for parity with the upstream
// snippet — comment out the PostAsync call if you only want to demo the
// shape of the API.
//
// Run with:
//   INPUT_MYTOKEN=$GITHUB_TOKEN \
//   GITHUB_REPOSITORY=octocat/Hello-World \
//     dotnet run create-issue.cs

#:package ActionsToolkit.Core@*
#:package ActionsToolkit.Octokit@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.Core.Extensions;
using ActionsToolkit.Core.Services;
using ActionsToolkit.Octokit;
using ActionsToolkit.Octokit.Extensions;
using GitHub;
using Microsoft.Extensions.DependencyInjection;

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

using var provider = new ServiceCollection()
    .AddGitHubClientServices(myToken)
    .BuildServiceProvider();

var client = provider.GetRequiredService<GitHubClient>();
var context = provider.GetRequiredService<Context>();

var owner = context.Repo.Owner;
var repo = context.Repo.Repo;

core.WriteInfo($"Would create an issue in {owner}/{repo}.");

// Uncomment the block below to actually create the issue. Equivalent to
// the upstream `octokit.rest.issues.create({ ...context.repo, title, body })`.
//
// var body = new global::GitHub.Repos.Item.Item.Issues.IssuesPostRequestBody
// {
//     Title = new()
//     {
//         String = "New issue!",
//     },
//     Body = "Hello Universe!",
// };
//
// var newIssue = await client.Repos[owner][repo].Issues.PostAsync(body);
// core.WriteInfo($"Created issue #{newIssue?.Number}: {newIssue?.HtmlUrl}");
