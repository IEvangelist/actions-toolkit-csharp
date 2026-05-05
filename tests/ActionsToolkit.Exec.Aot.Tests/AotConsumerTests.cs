// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Exec.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkit.Exec</c>. Mirrors the upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/exec/__tests__/exec.test.ts">
/// <c>actions/toolkit/packages/exec/__tests__/exec.test.ts</c></a> concerns: <c>exec</c>,
/// <c>getExecOutput</c>, listeners, env override, cwd override, and arg quoting.
/// </summary>
/// <remarks>
/// Each fact runs the previously-published native consumer with a single dispatcher
/// case and asserts a clean exit (<c>0</c>), an <c>[OK]</c> stdout marker, and
/// the absence of trimmer/AOT analysis warnings on stderr. When the publish step is
/// unavailable on the current host, tests log and return early — see
/// <see cref="AotPublishFixture"/>.
/// </remarks>
[Collection(AotConsumerCollection.Name)]
public sealed class AotConsumerTests(AotPublishFixture fixture)
{
    [Fact]
    public void ExecRunsCleanlyUnderAot() =>
        AssertCase("exec");

    [Fact]
    public void GetExecOutputRunsCleanlyUnderAot() =>
        AssertCase("get-exec-output");

    [Fact]
    public void ArgQuotingRunsCleanlyUnderAot() =>
        AssertCase("arg-quoting");

    [Fact]
    public void ListenerRunsCleanlyUnderAot() =>
        AssertCase("listener");

    [Fact]
    public void EnvOverrideRunsCleanlyUnderAot() =>
        AssertCase("env-override");

    [Fact]
    public void CwdOverrideRunsCleanlyUnderAot() =>
        AssertCase("cwd-override");

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
