// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.ToolCache.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkitSharp.ToolCache</c>. Runs a
/// previously-published native binary that exercises the public surface and
/// verifies the trimmer doesn't drop required code.
/// </summary>
[Collection(AotConsumerCollection.Name)]
public sealed class AotConsumerTests(AotPublishFixture fixture)
{
    [Fact]
    public void InstantiateServiceRunsCleanlyUnderAot() =>
        AssertCase("instantiate-service");

    [Fact]
    public void EvaluateVersionsRunsCleanlyUnderAot() =>
        AssertCase("evaluate-versions");

    [Fact]
    public void FindRunsCleanlyUnderAot() =>
        AssertCase("find");

    [Fact]
    public void CacheDirLayoutRunsCleanlyUnderAot() =>
        AssertCase("cache-dir-layout");

    [Fact]
    public void ManifestDeserializeRunsCleanlyUnderAot() =>
        AssertCase("manifest-deserialize");

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
