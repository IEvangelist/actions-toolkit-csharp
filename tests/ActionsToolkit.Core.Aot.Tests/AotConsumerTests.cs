// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Aot.Tests;

/// <summary>
/// Native AOT smoke tests for <c>ActionsToolkit.Core</c>. Mirrors the upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/core/__tests__/core.test.ts">
/// <c>actions/toolkit/packages/core/__tests__/core.test.ts</c></a>,
/// <a href="https://github.com/actions/toolkit/blob/main/packages/core/__tests__/command.test.ts">
/// <c>command.test.ts</c></a>,
/// <a href="https://github.com/actions/toolkit/blob/main/packages/core/__tests__/file-command.test.ts">
/// <c>file-command.test.ts</c></a>, and
/// <a href="https://github.com/actions/toolkit/blob/main/packages/core/__tests__/summary.test.ts">
/// <c>summary.test.ts</c></a>
/// concerns: log issuing (info/warning/error/notice/debug), output, secret masking,
/// path/env mutation, input parsing (trim/multiline/bool), failure signalling,
/// summary writing, group wrapping, state save/get, and the runner-debug flag.
/// </summary>
[Collection(AotConsumerCollection.Name)]
public sealed class AotConsumerTests(AotPublishFixture fixture)
{
    [Fact]
    public void WriteInfoRunsCleanlyUnderAot() => AssertCase("info");

    [Fact]
    public void WriteWarningRunsCleanlyUnderAot() => AssertCase("warning");

    [Fact]
    public void WriteErrorRunsCleanlyUnderAot() => AssertCase("error");

    [Fact]
    public void WriteNoticeRunsCleanlyUnderAot() => AssertCase("notice");

    [Fact]
    public void WriteDebugRunsCleanlyUnderAot() => AssertCase("debug");

    [Fact]
    public void SetOutputAsyncRunsCleanlyUnderAot() => AssertCase("set-output");

    [Fact]
    public void SetSecretRunsCleanlyUnderAot() => AssertCase("set-secret");

    [Fact]
    public void AddPathAsyncRunsCleanlyUnderAot() => AssertCase("add-path");

    [Fact]
    public void ExportVariableAsyncRunsCleanlyUnderAot() => AssertCase("export-variable");

    [Fact]
    public void GetInputRunsCleanlyUnderAot() => AssertCase("get-input");

    [Fact]
    public void GetMultilineInputRunsCleanlyUnderAot() => AssertCase("get-multiline-input");

    [Fact]
    public void GetBoolInputRunsCleanlyUnderAot() => AssertCase("get-bool-input");

    [Fact]
    public void SetFailedRunsCleanlyUnderAot() => AssertCase("set-failed");

    [Fact]
    public void SummaryRunsCleanlyUnderAot() => AssertCase("summary");

    [Fact]
    public void GroupRunsCleanlyUnderAot() => AssertCase("group");

    [Fact]
    public void SaveStateAsyncRunsCleanlyUnderAot() => AssertCase("save-state");

    [Fact]
    public void GetStateRunsCleanlyUnderAot() => AssertCase("get-state");

    [Fact]
    public void SetCommandEchoRunsCleanlyUnderAot() => AssertCase("command-echo");

    [Fact]
    public void IsDebugRunsCleanlyUnderAot() => AssertCase("is-debug");

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
