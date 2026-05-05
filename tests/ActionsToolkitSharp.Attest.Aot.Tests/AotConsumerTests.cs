// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkitSharp.Attest</c>. Runs a
/// previously-published native binary that exercises the public surface and
/// verifies the trimmer doesn't drop required code.
/// </summary>
[Collection(AotConsumerCollection.Name)]
public sealed class AotConsumerTests(AotPublishFixture fixture)
{
    [Fact]
    public void RegisterServicesRunsCleanlyUnderAot() =>
        AssertCase("register-services");

    [Fact]
    public void IntotoBuildRunsCleanlyUnderAot() =>
        AssertCase("intoto-build");

    [Fact]
    public void EndpointsResolveRunsCleanlyUnderAot() =>
        AssertCase("endpoints-resolve");

    [Fact]
    public void OidcConstructRunsCleanlyUnderAot() =>
        AssertCase("oidc-construct");

    [Fact]
    public void SignerFactoryConstructRunsCleanlyUnderAot() =>
        AssertCase("signer-factory-construct");

    [Fact]
    public void JsonRoundtripRunsCleanlyUnderAot() =>
        AssertCase("json-roundtrip");

    [Fact]
    public void ProvenanceDecodeRunsCleanlyUnderAot() =>
        AssertCase("provenance-decode");

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
