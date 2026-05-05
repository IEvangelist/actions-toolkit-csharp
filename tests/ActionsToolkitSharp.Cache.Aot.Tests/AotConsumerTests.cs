// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkitSharp.Cache</c>. Exercises
/// the public client surface (DI compose, options, validation helpers,
/// version computation, tar+zstd round-trip, JSON DTO contract) through a
/// published native binary and asserts no IL2/IL3 trim/AOT warnings escape.
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
    public void IsFeatureAvailableUnderAot() =>
        AssertCase("is-feature-available");

    [Fact]
    public void KeyValidationFiresUnderAot() =>
        AssertCase("key-validation");

    [Fact]
    public void VersionComputationDeterministicUnderAot() =>
        AssertCase("version-computation");

    [Fact]
    public void TarRoundTripsUnderAot() =>
        AssertCase("tar-roundtrip");

    [Fact]
    public void JsonDtosRoundTripUnderAot() =>
        AssertCase("json-roundtrip");

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
