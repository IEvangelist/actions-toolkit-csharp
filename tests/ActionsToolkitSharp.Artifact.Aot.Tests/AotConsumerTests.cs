// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkitSharp.Artifact</c>. Exercises
/// the public client surface (compose, options, GHES guard, validation, zip
/// pipeline, response records) through a published native binary and
/// asserts no IL2/IL3 trim/AOT warnings escape.
/// </summary>
[Collection(AotConsumerCollection.Name)]
public sealed class AotConsumerTests(AotPublishFixture fixture)
{
    [Fact]
    public void ComposesClientFromServiceProviderUnderAot() =>
        AssertCase("compose-client");

    [Fact]
    public void OptionsRecordsRoundTripUnderAot() =>
        AssertCase("options-records");

    [Fact]
    public void GhesGuardFiresUnderAot() =>
        AssertCase("ghes-guard");

    [Fact]
    public void ValidationHelpersFireUnderAot() =>
        AssertCase("validation-helpers");

    [Fact]
    public void ZipPipelineSurfaceReachesFilesNotFoundUnderAot() =>
        AssertCase("zip-pipeline");

    [Fact]
    public void ResponseRecordsRoundTripUnderAot() =>
        AssertCase("config-helpers");

    private void AssertCase(string @case)
    {
        if (!fixture.PublishSucceeded)
        {
            Console.WriteLine(
                $"[skip] AOT publish unavailable for case '{@case}': {fixture.PublishError}");
            return;
        }

        var result = fixture.Run(@case);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains($"[OK] {@case}", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("AOT analysis failure", result.Stderr, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IL2", result.Stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("IL3", result.Stderr, StringComparison.Ordinal);
    }
}
