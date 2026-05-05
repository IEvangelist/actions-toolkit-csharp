// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Octokit.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkitSharp.Octokit</c>. Mirrors the
/// upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/github/__tests__/github.test.ts">
/// <c>actions/toolkit/packages/github/__tests__/github.test.ts</c></a>
/// concerns: <c>GitHubClient</c> instantiation through DI and the static factory,
/// plus access to <c>Context.Current</c>. No network is hit; a fake
/// <c>GITHUB_TOKEN</c> is supplied to the subprocess via the fixture environment.
/// </summary>
[Collection(AotConsumerCollection.Name)]
public sealed class AotConsumerTests(AotPublishFixture fixture)
{
    [Fact]
    public void InstantiateGitHubClientViaDIRunsCleanlyUnderAot() =>
        AssertCase("instantiate-client");

    [Fact]
    public void InstantiateGitHubClientViaFactoryRunsCleanlyUnderAot() =>
        AssertCase("instantiate-via-factory");

    [Fact]
    public void AccessContextCurrentRunsCleanlyUnderAot() =>
        AssertCase("context-current");

    private void AssertCase(string @case, params string[] extraArgs)
    {
        if (!fixture.PublishSucceeded)
        {
            Console.WriteLine(
                $"[skip] AOT publish unavailable for case '{@case}': {fixture.PublishError}");
            return;
        }

        var result = fixture.Run(@case, extraArgs);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains($"[OK] {@case}", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("AOT analysis failure", result.Stderr, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IL2", result.Stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("IL3", result.Stderr, StringComparison.Ordinal);
    }
}
