// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Exec.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/exec/__tests__/exec.test.ts"/>.
/// Covers the headline <c>exec()</c> happy-path scenarios — running a tool with the args split out, partially split out,
/// or fully embedded in the command line, and resolving a tool from PATH. The upstream tests use
/// <c>cmd</c> on Windows and <c>ls</c> on Unix; here we use <c>cmd /c echo</c> on Windows and
/// <c>/bin/sh -c</c> on Unix because <c>ls</c> is not guaranteed available everywhere we test.
/// </summary>
public sealed class BasicExecTests
{
    [Fact(DisplayName = "Runs exec successfully with arguments split out")]
    public async Task RunsExecSuccessfullyWithArgumentsSplitOut()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.EchoCommand("hello");

        var exitCode = await exec.ExecAsync(commandLine, args, new ExecOptions { Silent = true });

        Assert.Equal(0, exitCode);
    }

    [Fact(DisplayName = "Runs exec successfully with arguments partially split out")]
    public async Task RunsExecSuccessfullyWithArgumentsPartiallySplitOut()
    {
        var exec = BuildExec();
        string[] args;
        string commandLine;
        if (TestPlatform.IsWindows)
        {
            commandLine = "cmd /c";
            args = ["echo", "hello"];
        }
        else
        {
            commandLine = "/bin/sh -c";
            args = ["printf 'hello\\n'"];
        }

        var exitCode = await exec.ExecAsync(commandLine, args, new ExecOptions { Silent = true });

        Assert.Equal(0, exitCode);
    }

    [Fact(DisplayName = "Runs exec successfully with arguments as part of command line")]
    public async Task RunsExecSuccessfullyWithArgumentsAsPartOfCommandLine()
    {
        var exec = BuildExec();
        var commandLine = TestPlatform.IsWindows
            ? "cmd /c echo hello"
            : "/bin/sh -c \"printf 'hello\\n'\"";

        var exitCode = await exec.ExecAsync(commandLine, args: null, new ExecOptions { Silent = true });

        Assert.Equal(0, exitCode);
    }

    [Fact(DisplayName = "Runs exec successfully with command from PATH")]
    public async Task RunsExecSuccessfullyWithCommandFromPath()
    {
        var exec = BuildExec();
        var stdout = new StringBuilder();
        var (commandLine, args) = TestPlatform.EchoCommand("hello");

        var options = new ExecOptions
        {
            Silent = true,
            Listeners = new ExecListeners
            {
                Stdout = data => stdout.Append(Encoding.UTF8.GetString(data.Span)),
            },
        };

        var exitCode = await exec.ExecAsync(commandLine, args, options);

        Assert.Equal(0, exitCode);
        Assert.Contains("hello", stdout.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Exec fails with error on bad call")]
    public async Task ExecFailsWithErrorOnBadCall()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.FailingCommand(2);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await exec.ExecAsync(commandLine, args, new ExecOptions { Silent = true }));

        Assert.Contains("failed with exit code", ex.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Succeeds on stderr by default")]
    public async Task SucceedsOnStderrByDefault()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.StderrCommand("this is output to stderr");

        var exitCode = await exec.ExecAsync(commandLine, args, new ExecOptions { Silent = true });

        Assert.Equal(0, exitCode);
    }

    [Fact(DisplayName = "Fails on stderr if specified")]
    public async Task FailsOnStderrIfSpecified()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.StderrCommand("this is output to stderr");

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await exec.ExecAsync(commandLine, args,
                new ExecOptions { Silent = true, FailOnStdErr = true }));
    }

    [Fact(DisplayName = "Fails when process fails to launch")]
    public async Task FailsWhenProcessFailsToLaunch()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.EchoCommand("hello");
        var bogusCwd = Path.Combine(Path.GetTempPath(), $"ats-exec-nosuchdir-{Guid.NewGuid():N}");

        await Assert.ThrowsAnyAsync<Exception>(
            async () => await exec.ExecAsync(commandLine, args,
                new ExecOptions { Silent = true, Cwd = bogusCwd }));
    }

    private static IExecService BuildExec()
    {
        return new ServiceCollection()
            .AddGitHubActionsExec()
            .BuildServiceProvider()
            .GetRequiredService<IExecService>();
    }
}
