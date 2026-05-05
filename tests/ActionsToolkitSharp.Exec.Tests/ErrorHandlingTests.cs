// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Exec.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/exec/__tests__/exec.test.ts"/>.
/// Validates the runner's failure-mode contracts — non-zero exit codes, <c>FailOnStdErr</c>,
/// <c>IgnoreReturnCode</c>, missing-cwd, missing-tool, and cancellation propagation.
/// </summary>
public sealed class ErrorHandlingTests
{
    [Fact(DisplayName = "Non-zero exit code throws by default")]
    public async Task NonZeroExitCodeThrowsByDefault()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.FailingCommand(7);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await exec.ExecAsync(commandLine, args, new ExecOptions { Silent = true }));

        Assert.Contains("failed with exit code 7", ex.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "ignoreReturnCode swallows non-zero exit code")]
    public async Task IgnoreReturnCodeSwallowsNonZeroExitCode()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.FailingCommand(42);

        var exitCode = await exec.ExecAsync(commandLine, args,
            new ExecOptions { Silent = true, IgnoreReturnCode = true });

        Assert.Equal(42, exitCode);
    }

    [Fact(DisplayName = "Exec roots throws friendly error when bad cwd is specified")]
    public async Task ExecRootsThrowsFriendlyErrorWhenBadCwdIsSpecified()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.EchoCommand("hello");
        var bogusCwd = Path.Combine(Path.GetTempPath(), $"ats-exec-nosuchdir-{Guid.NewGuid():N}");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await exec.ExecAsync(commandLine, args,
                new ExecOptions { Silent = true, Cwd = bogusCwd }));

        Assert.Contains("does not exist", ex.Message, StringComparison.Ordinal);
        Assert.Contains(bogusCwd, ex.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Empty commandLine throws ArgumentException")]
    public async Task EmptyCommandLineThrowsArgumentException()
    {
        var exec = BuildExec();

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await exec.ExecAsync("", []));
    }

    [Fact(DisplayName = "Missing tool throws FileNotFoundException")]
    public async Task MissingToolThrowsFileNotFoundException()
    {
        var exec = BuildExec();

        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await exec.ExecAsync("definitely-not-a-real-tool-ats", []));
    }

    [Fact(DisplayName = "Cancellation token kills the process")]
    public async Task CancellationTokenKillsTheProcess()
    {
        var exec = BuildExec();
        // A long-running command that we cancel almost immediately.
        string commandLine;
        string[] args;
        if (TestPlatform.IsWindows)
        {
            commandLine = "cmd";
            args = ["/c", "ping", "-n", "30", "127.0.0.1"];
        }
        else
        {
            commandLine = "/bin/sh";
            args = ["-c", "sleep 30"];
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await exec.ExecAsync(commandLine, args,
                new ExecOptions { Silent = true, IgnoreReturnCode = true }, cts.Token));
    }

    private static IExecService BuildExec()
    {
        return new ServiceCollection()
            .AddGitHubActionsExec()
            .BuildServiceProvider()
            .GetRequiredService<IExecService>();
    }
}
