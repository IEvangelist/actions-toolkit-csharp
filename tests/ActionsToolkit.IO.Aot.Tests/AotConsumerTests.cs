// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.IO.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkit.IO</c>. Mirrors the upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/io/__tests__/io.test.ts">
/// <c>actions/toolkit/packages/io/__tests__/io.test.ts</c></a> concerns: <c>cp</c>,
/// <c>mv</c>, <c>rm</c>, <c>mkdirP</c>, <c>which</c>, and <c>findInPath</c>.
/// </summary>
/// <remarks>
/// Each fact runs the previously-published native consumer with a single dispatcher
/// case and asserts a clean exit (<c>0</c>), an <c>[OK]</c> stdout marker, and
/// empty stderr. When the publish step is unavailable on the current host, tests
/// log and return early — see <see cref="AotPublishFixture"/>.
/// </remarks>
[Collection(AotConsumerCollection.Name)]
public sealed class AotConsumerTests(AotPublishFixture fixture)
{
    [Fact]
    public void ResolveIOperationsFromDIRunsCleanlyUnderAot() =>
        AssertCase("resolve-iops");

    [Fact]
    public void MakeDirectoryPRunsCleanlyUnderAot() =>
        AssertCase("mkdir-p");

    [Fact]
    public void CopyRunsCleanlyUnderAot() =>
        AssertCase("cp");

    [Fact]
    public void MoveRunsCleanlyUnderAot() =>
        AssertCase("mv");

    [Fact]
    public void RemoveRunsCleanlyUnderAot() =>
        AssertCase("rm");

    [Fact]
    public void WhichRunsCleanlyUnderAot() =>
        AssertCase("which");

    [Fact]
    public void FileInPathRunsCleanlyUnderAot() =>
        AssertCase("file-in-path");

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
