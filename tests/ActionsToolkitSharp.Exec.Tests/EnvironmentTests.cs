// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Exec.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/exec/__tests__/exec.test.ts"/>.
/// Mirrors upstream cwd / env override concerns (<c>Exec roots relative tool path using rooted options.cwd</c>,
/// <c>does not throw when valid cwd is provided</c>, env-passthrough behavior implied by <c>options.env</c>).
/// </summary>
public sealed class EnvironmentTests
{
    [Fact(DisplayName = "Exec roots does not throw when valid cwd is provided")]
    public async Task ExecRootsDoesNotThrowWhenValidCwdIsProvided()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.EchoCommand("hello");

        var exitCode = await exec.ExecAsync(commandLine, args,
            new ExecOptions { Silent = true, Cwd = Path.GetTempPath() });

        Assert.Equal(0, exitCode);
    }

    [Fact(DisplayName = "options.env replaces inherited environment")]
    public async Task OptionsEnvReplacesInheritedEnvironment()
    {
        var exec = BuildExec();

        // We set ATS_EXEC_TEST_VAR in the parent and expect the child not to see it
        // (because we override env explicitly).
        Environment.SetEnvironmentVariable("ATS_EXEC_TEST_VAR", "parent-value");
        try
        {
            string commandLine;
            string[] args;
            if (TestPlatform.IsWindows)
            {
                // On Windows, %ATS_EXEC_TEST_VAR% expands at parse time; use cmd /c set ATS_EXEC_TEST_VAR
                commandLine = "cmd";
                args = ["/c", "set", "ATS_EXEC_TEST_VAR"];
            }
            else
            {
                commandLine = "/bin/sh";
                args = ["-c", "echo \"VAR=${ATS_EXEC_TEST_VAR-<unset>}\""];
            }

            var output = await exec.GetExecOutputAsync(commandLine, args, new ExecOptions
            {
                Silent = true,
                IgnoreReturnCode = true, // `set NAME` exits 1 when NAME is undefined on cmd.
                Env = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    // Provide a minimal env: PATH so the child can find itself, plus SystemRoot for cmd.exe.
                    ["PATH"] = Environment.GetEnvironmentVariable("PATH") ?? "",
                    ["SystemRoot"] = Environment.GetEnvironmentVariable("SystemRoot") ?? "",
                },
            });

            // Either the var is reported missing, or stdout is empty (set NAME with no match prints "Environment variable NAME not defined").
            Assert.DoesNotContain("parent-value", output.Stdout, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ATS_EXEC_TEST_VAR", null);
        }
    }

    [Fact(DisplayName = "options.env makes provided variables visible to the child")]
    public async Task OptionsEnvMakesProvidedVariablesVisibleToTheChild()
    {
        var exec = BuildExec();
        string commandLine;
        string[] args;
        if (TestPlatform.IsWindows)
        {
            commandLine = "cmd";
            args = ["/c", "echo", "%ATS_EXEC_CUSTOM%"];
        }
        else
        {
            commandLine = "/bin/sh";
            args = ["-c", "printf '%s' \"$ATS_EXEC_CUSTOM\""];
        }

        var output = await exec.GetExecOutputAsync(commandLine, args, new ExecOptions
        {
            Silent = true,
            Env = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["PATH"] = Environment.GetEnvironmentVariable("PATH") ?? "",
                ["SystemRoot"] = Environment.GetEnvironmentVariable("SystemRoot") ?? "",
                ["ATS_EXEC_CUSTOM"] = "child-only-value",
            },
        });

        Assert.Equal(0, output.ExitCode);
        Assert.Contains("child-only-value", output.Stdout, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "options.input writes bytes to child stdin")]
    public async Task OptionsInputWritesBytesToChildStdin()
    {
        var exec = BuildExec();
        // Use sh's `cat` on Unix; on Windows we skip because PowerShell stdin piping is racy in CI.
        if (TestPlatform.IsWindows)
        {
            return;
        }

        var (commandLine, args) = TestPlatform.CatStdinCommand();
        var input = "hello-from-stdin"u8.ToArray();

        var output = await exec.GetExecOutputAsync(commandLine, args, new ExecOptions
        {
            Silent = true,
            Input = input,
        });

        Assert.Equal(0, output.ExitCode);
        Assert.Contains("hello-from-stdin", output.Stdout, StringComparison.Ordinal);
    }

    private static IExecService BuildExec()
    {
        return new ServiceCollection()
            .AddGitHubActionsExec()
            .BuildServiceProvider()
            .GetRequiredService<IExecService>();
    }
}
