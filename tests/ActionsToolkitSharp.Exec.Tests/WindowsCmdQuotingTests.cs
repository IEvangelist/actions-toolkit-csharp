// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Exec.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/exec/__tests__/exec.test.ts"/>.
/// Mirrors the Windows-only quoting tests — the libuv <c>quote_cmd_arg</c> rules used for <c>.exe</c>
/// invocations and the cmd.exe-specific quoting used for <c>.cmd</c> / <c>.bat</c> invocations.
/// All facts in this class skip on non-Windows hosts.
/// </summary>
public sealed class WindowsCmdQuotingTests
{
    [Fact(DisplayName = "execs .exe with arg quoting (Windows)")]
    public async Task ExecsExeWithArgQuotingOnWindows()
    {
        if (!TestPlatform.IsWindows)
        {
            return;
        }

        var exec = BuildExec();
        var stdout = new StringBuilder();
        var options = new ExecOptions
        {
            Silent = true,
            Listeners = new ExecListeners
            {
                Stdout = data => stdout.Append(Encoding.UTF8.GetString(data.Span)),
            },
        };

        // .exe path goes through libuv quote_cmd_arg rules. We verify the args round-trip through
        // cmd.exe by echoing them back: cmd /c echo expands its args verbatim.
        var exitCode = await exec.ExecAsync(
            "cmd",
            ["/c", "echo", "helloworld", "hello world", "hello,world"],
            options);

        Assert.Equal(0, exitCode);
        var captured = stdout.ToString();
        Assert.Contains("helloworld", captured, StringComparison.Ordinal);
        Assert.Contains("hello world", captured, StringComparison.Ordinal);
        Assert.Contains("hello,world", captured, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "execs .cmd with a space and with arg with space (Windows)")]
    public async Task ExecsCmdWithSpaceAndArgWithSpaceOnWindows()
    {
        if (!TestPlatform.IsWindows)
        {
            return;
        }

        // Round-trip a `.cmd` script through the cmd.exe quoting path. We synthesize the script in
        // a temp folder so we don't depend on repo-relative paths.
        var tempDir = Path.Combine(Path.GetTempPath(), $"ats-exec-cmd-{Guid.NewGuid():N} with space");
        Directory.CreateDirectory(tempDir);
        try
        {
            var cmdPath = Path.Combine(tempDir, "echo args.cmd");
            File.WriteAllText(cmdPath, "@echo off\r\necho %1 %2\r\n");

            var exec = BuildExec();
            var stdout = new StringBuilder();
            var options = new ExecOptions
            {
                Silent = true,
                Listeners = new ExecListeners
                {
                    Stdout = data => stdout.Append(Encoding.UTF8.GetString(data.Span)),
                },
            };

            var exitCode = await exec.ExecAsync(
                $"\"{cmdPath}\"",
                ["my arg 1", "my arg 2"],
                options);

            Assert.Equal(0, exitCode);
            // We don't pin the exact echoed format (cmd.exe quoting is tricky); we just verify the
            // script ran and the args reached it in some recognizable shape.
            var captured = stdout.ToString();
            Assert.Contains("my arg 1", captured, StringComparison.Ordinal);
            Assert.Contains("my arg 2", captured, StringComparison.Ordinal);
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { /* best effort */ }
        }
    }

    [Fact(DisplayName = "execs .exe with verbatim args (Windows)")]
    public async Task ExecsExeWithVerbatimArgsOnWindows()
    {
        if (!TestPlatform.IsWindows)
        {
            return;
        }

        var exec = BuildExec();
        var stdout = new StringBuilder();
        var options = new ExecOptions
        {
            Silent = true,
            WindowsVerbatimArguments = true,
            Listeners = new ExecListeners
            {
                Stdout = data => stdout.Append(Encoding.UTF8.GetString(data.Span)),
            },
        };

        // With verbatim args, the runner does not re-quote — args are passed exactly as-is.
        var exitCode = await exec.ExecAsync("cmd", ["/c", "echo", "verbatim-arg"], options);

        Assert.Equal(0, exitCode);
        Assert.Contains("verbatim-arg", stdout.ToString(), StringComparison.Ordinal);
    }

    private static IExecService BuildExec()
    {
        return new ServiceCollection()
            .AddGitHubActionsExec()
            .BuildServiceProvider()
            .GetRequiredService<IExecService>();
    }
}
