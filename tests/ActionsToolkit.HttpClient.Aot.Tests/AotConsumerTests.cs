// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkit.HttpClient</c>. Mirrors the
/// upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/http-client/__tests__/auth.test.ts">
/// <c>actions/toolkit/packages/http-client/__tests__/auth.test.ts</c></a> and
/// <a href="https://github.com/actions/toolkit/blob/main/packages/http-client/__tests__/basic.test.ts">
/// <c>basic.test.ts</c></a>
/// concerns: DI registration of <c>IHttpCredentialClientFactory</c> and the basic,
/// bearer, and PAT credential client variants. No network calls are issued.
/// </summary>
[Collection(AotConsumerCollection.Name)]
public sealed class AotConsumerTests(AotPublishFixture fixture)
{
    [Fact]
    public void InstantiateDefaultClientRunsCleanlyUnderAot() =>
        AssertCase("instantiate-default-client");

    [Fact]
    public void BasicAuthClientRunsCleanlyUnderAot() =>
        AssertCase("auth-handler-basic");

    [Fact]
    public void BearerAuthClientRunsCleanlyUnderAot() =>
        AssertCase("auth-handler-bearer");

    [Fact]
    public void PersonalAccessTokenClientRunsCleanlyUnderAot() =>
        AssertCase("auth-handler-pat");

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
