// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Tests;

/// <summary>
/// Mirrors the upstream <c>evaluateVersions</c> tests in
/// <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/__tests__/tool-cache.test.ts">
/// <c>tool-cache.test.ts</c></a> §"evaluateVersions".
/// </summary>
public sealed class EvaluateVersionsTests
{
    private static readonly IToolCacheService s_service =
        new DefaultToolCacheService(
            httpClient: new StubHttpClient(new System.Net.Http.HttpClient()),
            retryHelper: new DefaultRetryHelper(maxAttempts: 1, minSeconds: 0, maxSeconds: 0));

    [Fact(DisplayName = "evaluate matching version")]
    public void EvaluateMatchingVersion()
    {
        string[] versions = ["1.0.0", "1.1.0", "2.0.0"];
        var match = s_service.EvaluateVersions(versions, "1.x");
        Assert.Equal("1.1.0", match);
    }

    [Fact(DisplayName = "evaluate caret matching version")]
    public void EvaluateCaretMatchingVersion()
    {
        string[] versions = ["1.0.0", "1.1.0", "2.0.0"];
        var match = s_service.EvaluateVersions(versions, "^1.0.0");
        Assert.Equal("1.1.0", match);
    }

    [Fact(DisplayName = "evaluate tilde matching version")]
    public void EvaluateTildeMatchingVersion()
    {
        string[] versions = ["1.0.0", "1.0.5", "1.1.0", "2.0.0"];
        var match = s_service.EvaluateVersions(versions, "~1.0.0");
        Assert.Equal("1.0.5", match);
    }

    [Fact(DisplayName = "evaluate latest")]
    public void EvaluateLatest()
    {
        string[] versions = ["1.0.0", "1.1.0", "2.0.0", "3.0.0-alpha"];
        var match = s_service.EvaluateVersions(versions, "*");
        Assert.Equal("2.0.0", match);
    }

    [Fact(DisplayName = "evaluate exact version")]
    public void EvaluateExactVersion()
    {
        string[] versions = ["1.0.0", "1.1.0", "2.0.0"];
        var match = s_service.EvaluateVersions(versions, "1.1.0");
        Assert.Equal("1.1.0", match);
    }

    [Fact(DisplayName = "evaluate no match returns null")]
    public void EvaluateNoMatchReturnsNull()
    {
        string[] versions = ["1.0.0", "1.1.0", "2.0.0"];
        var match = s_service.EvaluateVersions(versions, "3.x");
        Assert.Null(match);
    }

    [Fact(DisplayName = "evaluate ignores invalid versions")]
    public void EvaluateIgnoresInvalidVersions()
    {
        string[] versions = ["1.0.0", "not-a-version", "1.1.0"];
        var match = s_service.EvaluateVersions(versions, "1.x");
        Assert.Equal("1.1.0", match);
    }

    [Fact(DisplayName = "evaluate empty input returns null")]
    public void EvaluateEmptyInputReturnsNull()
    {
        var match = s_service.EvaluateVersions([], "1.x");
        Assert.Null(match);
    }

    [Fact(DisplayName = "evaluate v-prefixed version")]
    public void EvaluateVPrefixedVersion()
    {
        string[] versions = ["v1.0.0", "v1.1.0", "v2.0.0"];
        var match = s_service.EvaluateVersions(versions, "1.x");
        Assert.Equal("v1.1.0", match);
    }

    [Fact(DisplayName = "evaluate hyphen range")]
    public void EvaluateHyphenRange()
    {
        string[] versions = ["1.0.0", "1.5.0", "2.0.0", "2.5.0", "3.0.0"];
        var match = s_service.EvaluateVersions(versions, "1.0.0 - 2.0.0");
        Assert.Equal("2.0.0", match);
    }

    [Fact(DisplayName = "evaluate or expression")]
    public void EvaluateOrExpression()
    {
        string[] versions = ["1.0.0", "2.0.0", "3.0.0"];
        var match = s_service.EvaluateVersions(versions, "1.x || 2.x");
        Assert.Equal("2.0.0", match);
    }
}
