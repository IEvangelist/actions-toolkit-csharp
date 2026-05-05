// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Octokit.Tests;

public class GitHubClientTests
{
    [Fact]
    public async Task GitHubClientGetsFirstPullRequestTest()
    {
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (token is null)
        {
            return; // Skip the test if the token isn't available.
        }

        var client = GitHubClientFactory.Create(token);

        var owner = "IEvangelist";
        var repo = "actions-toolkit-csharp";
        var pullNumber = 1;

        var firstPullRequest = await client.Repos[owner][repo].Pulls[pullNumber].GetAsync();

        Assert.NotNull(firstPullRequest);
    }

    [Fact(Skip = "Not an actual test...")]
    public async Task RawHttpClientRestTest()
    {
        var client = new HttpClient();

        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        client.DefaultRequestHeaders.UserAgent.Add(
            new("Test", "0.1"));
        client.DefaultRequestHeaders.Authorization =
            new("Bearer", token);

        var owner = "DecimalTurn";
        var repo = "VBA-on-GitHub-Automations";
        var issueCommentId = 2310943199;

        try
        {
            var response = await client.GetAsync($"""
                https://api.github.com/repos/{owner}/{repo}/issues/comments/{issueCommentId}
                """);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            if (json is { })
            {

            }
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Fact(Skip = "Upstream GitHub issue: https://github.com/octokit/dotnet-sdk/issues/117")]
    public async Task GitHubClientGetsIssueCommentTest()
    {
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (token is null)
        {
            return; // Skip the test if the token isn't available.
        }

        var client = GitHubClientFactory.Create(token);

        var owner = "DecimalTurn";
        var repo = "VBA-on-GitHub-Automations";
        var issueCommentId = 2310943199;

        try
        {
            var issueComment =
            await client.Repos[owner][repo].Issues.Comments[(int)issueCommentId].GetAsync();

            Assert.NotNull(issueComment);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}
