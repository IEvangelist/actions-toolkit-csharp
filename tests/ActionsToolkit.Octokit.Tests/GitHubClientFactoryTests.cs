// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Octokit.Tests;

public class GitHubClientFactoryTests
{
    [Fact]
    public void CreateThrowsOnNullToken()
    {
        Assert.Throws<ArgumentNullException>(() => GitHubClientFactory.Create(null!));
    }

    [Fact]
    public void CreateThrowsOnEmptyToken()
    {
        Assert.Throws<ArgumentException>(() => GitHubClientFactory.Create(string.Empty));
    }

    [Fact]
    public void CreateThrowsOnWhitespaceToken()
    {
        Assert.Throws<ArgumentException>(() => GitHubClientFactory.Create(" "));
    }

    [Fact]
    public void CreateReturnsClientOnFakeToken()
    {
        Assert.NotNull(GitHubClientFactory.Create("token"));
    }
}
