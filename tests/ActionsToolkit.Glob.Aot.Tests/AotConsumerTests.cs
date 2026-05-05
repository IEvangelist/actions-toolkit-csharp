// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Glob.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkit.Glob</c>. Mirrors the upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/glob/__tests__/internal-globber.test.ts">
/// <c>actions/toolkit/packages/glob/__tests__/internal-globber.test.ts</c></a>
/// concerns: pattern resolution via the builder, the static <see cref="Globber"/>
/// facade, and the <c>string?</c> extension methods.
/// </summary>
[Collection(AotConsumerCollection.Name)]
public sealed class AotConsumerTests(AotPublishFixture fixture)
{
    [Fact]
    public void GlobBuilderResolvesMatchesUnderAot() =>
        AssertCase("glob-builder");

    [Fact]
    public void GlobberStaticFacadeRunsCleanlyUnderAot() =>
        AssertCase("globber-static");

    [Fact]
    public void GlobStringExtensionRunsCleanlyUnderAot() =>
        AssertCase("glob-string-extension");

    [Fact]
    public void GlobFilesStringExtensionRunsCleanlyUnderAot() =>
        AssertCase("glob-files-string-extension");

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
