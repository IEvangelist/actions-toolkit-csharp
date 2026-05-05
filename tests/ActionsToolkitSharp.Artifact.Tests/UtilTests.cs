// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Mirrors upstream
/// <c>actions/toolkit/packages/artifact/__tests__/util.test.ts</c>.
/// Covers <see cref="ActionsRuntimeJwt"/> JWT scope parsing and
/// <see cref="EnvironmentBackendIdsProvider"/> env-var reading. The upstream
/// <c>maskSigUrl</c>/<c>maskSecretUrls</c> helpers are not part of the C#
/// port (we don't have a public <c>setSecret</c> equivalent in this package),
/// so those upstream cases are intentionally not mirrored.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class UtilTests
{
    // The five tokens below are copied verbatim from upstream
    // __tests__/util.test.ts so that the C# parity tests run against the
    // exact same JWT payloads.
    private const string ValidRuntimeToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwic2NwIjoi" +
        "QWN0aW9ucy5FeGFtcGxlIEFjdGlvbnMuQW5vdGhlckV4YW1wbGU6dGVzdCBBY3Rpb25zLlJl" +
        "c3VsdHM6Y2U3ZjU0YzctNjFjNy00YWFlLTg4N2YtMzBkYTQ3NWY1ZjFhOmNhMzk1MDg1LTA0" +
        "MGEtNTI2Yi0yY2U4LWJkYzg1ZjY5Mjc3NCIsImlhdCI6MTUxNjIzOTAyMn0.XYnI_wHPBlUi" +
        "1mqYveJnnkJhp4dlFjqxzRmISPsqfw8";

    private const string TokenWithoutResultsScope =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwic2NwIjoi" +
        "QWN0aW9ucy5FeGFtcGxlIEFjdGlvbnMuQW5vdGhlckV4YW1wbGU6dGVzdCIsImlhdCI6MTUx" +
        "NjIzOTAyMn0.K0IEoULZteGevF38G94xiaA8zcZ5UlKWfGfqE6q3dhw";

    private const string TokenWithMalformedScope =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwic2NwIjoi" +
        "QWN0aW9ucy5FeGFtcGxlIEFjdGlvbnMuQW5vdGhlckV4YW1wbGU6dGVzdCBBY3Rpb25zLlJl" +
        "c3VsdHM6Y2U3ZjU0YzctNjFjNy00YWFlLTg4N2YtMzBkYTQ3NWY1ZjFhIiwiaWF0IjoxNTE2" +
        "MjM5MDIyfQ.7D0_LRfRFRZFImHQ7GxH2S6ZyFjjZ5U0ujjGCfle1XE";

    private const string TokenWithoutScpClaim =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6" +
        "IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV" +
        "_adQssw5c";

    private const string ExpectedWorkflowRunBackendId =
        "ce7f54c7-61c7-4aae-887f-30da475f5f1a";

    private const string ExpectedWorkflowJobRunBackendId =
        "ca395085-040a-526b-2ce8-bdc85f692774";

    [Fact(DisplayName = "should return backend ids when the token is valid")]
    public void ShouldReturnBackendIdsWhenTheTokenIsValid()
    {
        var ids = ActionsRuntimeJwt.ParseBackendIds(ValidRuntimeToken);

        Assert.Equal(ExpectedWorkflowRunBackendId, ids.WorkflowRunBackendId);
        Assert.Equal(ExpectedWorkflowJobRunBackendId, ids.WorkflowJobRunBackendId);
    }

    [Fact(DisplayName = "should throw an error when the token doesn't have the right scope")]
    public void ShouldThrowWhenTheTokenDoesntHaveTheRightScope() =>
        Assert.Throws<InvalidArtifactTokenException>(
            () => ActionsRuntimeJwt.ParseBackendIds(TokenWithoutResultsScope));

    [Fact(DisplayName = "should throw an error when the token has a malformed scope")]
    public void ShouldThrowWhenTheTokenHasAMalformedScope() =>
        Assert.Throws<InvalidArtifactTokenException>(
            () => ActionsRuntimeJwt.ParseBackendIds(TokenWithMalformedScope));

    [Fact(DisplayName = "should throw an error when the token is in an invalid format")]
    public void ShouldThrowWhenTheTokenIsInAnInvalidFormat() =>
        Assert.Throws<InvalidArtifactTokenException>(
            () => ActionsRuntimeJwt.ParseBackendIds("token"));

    [Fact(DisplayName = "should throw an error when the token doesn't have the right field")]
    public void ShouldThrowWhenTheTokenDoesntHaveTheRightField() =>
        Assert.Throws<InvalidArtifactTokenException>(
            () => ActionsRuntimeJwt.ParseBackendIds(TokenWithoutScpClaim));

    [Fact(DisplayName = "ParseBackendIds throws ArgumentNullException for a null token")]
    public void ParseBackendIdsThrowsForNullToken() =>
        Assert.Throws<ArgumentNullException>(
            () => ActionsRuntimeJwt.ParseBackendIds(null!));

    [Fact(DisplayName = "ParseBackendIds rejects a payload that is not base64url-decodable")]
    public void ParseBackendIdsRejectsBadBase64() =>
        Assert.Throws<InvalidArtifactTokenException>(
            () => ActionsRuntimeJwt.ParseBackendIds("aaa.!!!.bbb"));

    [Fact(DisplayName = "EnvironmentBackendIdsProvider reads ACTIONS_RUNTIME_TOKEN from the environment")]
    public void ProviderReadsEnvironmentVariable()
    {
        using var _ = new EnvironmentScope("ACTIONS_RUNTIME_TOKEN", ValidRuntimeToken);

        var ids = new EnvironmentBackendIdsProvider().Get();

        Assert.Equal(ExpectedWorkflowRunBackendId, ids.WorkflowRunBackendId);
        Assert.Equal(ExpectedWorkflowJobRunBackendId, ids.WorkflowJobRunBackendId);
    }

    [Fact(DisplayName = "EnvironmentBackendIdsProvider throws when ACTIONS_RUNTIME_TOKEN is missing")]
    public void ProviderThrowsWhenEnvironmentVariableMissing()
    {
        using var _ = new EnvironmentScope("ACTIONS_RUNTIME_TOKEN", value: null);

        var provider = new EnvironmentBackendIdsProvider();

        Assert.Throws<InvalidArtifactTokenException>(provider.Get);
    }

    [Fact(DisplayName = "EnvironmentBackendIdsProvider throws when ACTIONS_RUNTIME_TOKEN is whitespace")]
    public void ProviderThrowsWhenEnvironmentVariableIsWhitespace()
    {
        using var _ = new EnvironmentScope("ACTIONS_RUNTIME_TOKEN", "   ");

        var provider = new EnvironmentBackendIdsProvider();

        Assert.Throws<InvalidArtifactTokenException>(provider.Get);
    }

    [Fact(DisplayName = "EnvironmentBackendIdsProvider memoizes the parsed BackendIds across calls")]
    public void ProviderMemoizesParsedIds()
    {
        using var _ = new EnvironmentScope("ACTIONS_RUNTIME_TOKEN", ValidRuntimeToken);

        var provider = new EnvironmentBackendIdsProvider();
        var first = provider.Get();
        var second = provider.Get();

        Assert.Same(first, second);
    }
}
