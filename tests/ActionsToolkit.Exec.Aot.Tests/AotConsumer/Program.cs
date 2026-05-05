// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text;

using ActionsToolkit.Exec;
using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.Exec.AotConsumer;

/// <summary>
/// Native AOT dispatcher that exercises the public surface of
/// <see cref="ActionsToolkit.Exec"/> so that the trimmer roots every API.
/// Each <c>case</c> below mirrors a concern from the upstream
/// <c>actions/toolkit/packages/exec/__tests__/exec.test.ts</c> suite.
/// </summary>
internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: actions-toolkit-exec-aot-consumer <case> [sandbox-path]");
        }

        try
        {
            var services = new ServiceCollection()
                .AddGitHubActionsExec()
                .BuildServiceProvider();

            var exec = services.GetRequiredService<IExecService>();
            var sandbox = args.Length >= 2 ? args[1] : Path.Combine(Path.GetTempPath(), "actions-toolkit-exec-aot-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sandbox);

            return args[0] switch
            {
                "exec" => await RunExec(exec),
                "get-exec-output" => await RunGetExecOutput(exec),
                "arg-quoting" => await RunArgQuoting(exec),
                "listener" => await RunListener(exec),
                "env-override" => await RunEnvOverride(exec),
                "cwd-override" => await RunCwdOverride(exec, sandbox),
                _ => Fail($"Unknown case: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            return Fail($"{ex.GetType().FullName}: {ex.Message}");
        }
    }

    private static async Task<int> RunExec(IExecService exec)
    {
        var exit = await exec.ExecAsync("dotnet", ["--version"], new ExecOptions { Silent = true });
        return exit == 0 ? Ok("exec") : Fail($"exec returned {exit}");
    }

    private static async Task<int> RunGetExecOutput(IExecService exec)
    {
        var output = await exec.GetExecOutputAsync("dotnet", ["--version"], new ExecOptions { Silent = true });
        return output.ExitCode == 0 && output.Stdout.Length > 0
            ? Ok("get-exec-output", output.Stdout.Trim())
            : Fail($"get-exec-output unexpected: exit={output.ExitCode} stdout='{output.Stdout}' stderr='{output.Stderr}'");
    }

    private static async Task<int> RunArgQuoting(IExecService exec)
    {
        // Exec a command-line string that splits via argStringToArray; whitespace + quoted arg.
        var output = await exec.GetExecOutputAsync("dotnet --version", null, new ExecOptions { Silent = true });
        return output.ExitCode == 0 && output.Stdout.Length > 0
            ? Ok("arg-quoting", output.Stdout.Trim())
            : Fail($"arg-quoting unexpected: exit={output.ExitCode} stdout='{output.Stdout}' stderr='{output.Stderr}'");
    }

    private static async Task<int> RunListener(IExecService exec)
    {
        var captured = new StringBuilder();
        var options = new ExecOptions
        {
            Silent = true,
            Listeners = new ExecListeners
            {
                Stdout = data => captured.Append(Encoding.UTF8.GetString(data.Span)),
            },
        };
        var exit = await exec.ExecAsync("dotnet", ["--version"], options);
        return exit == 0 && captured.Length > 0
            ? Ok("listener", captured.ToString().Trim())
            : Fail($"listener unexpected: exit={exit} captured='{captured}'");
    }

    private static async Task<int> RunEnvOverride(IExecService exec)
    {
        var probeVar = "ATS_EXEC_AOT_PROBE";
        var probeValue = "from-aot-" + Guid.NewGuid().ToString("N");
        string commandLine;
        string[] args;
        if (OperatingSystem.IsWindows())
        {
            commandLine = "cmd";
            args = ["/c", $"echo %{probeVar}%"];
        }
        else
        {
            commandLine = "/bin/sh";
            args = ["-c", $"printf '%s\\n' \"${probeVar}\""];
        }

        var output = await exec.GetExecOutputAsync(commandLine, args, new ExecOptions
        {
            Silent = true,
            Env = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [probeVar] = probeValue,
            },
        });

        return output.ExitCode == 0 && output.Stdout.Contains(probeValue, StringComparison.Ordinal)
            ? Ok("env-override", probeValue)
            : Fail($"env-override unexpected: exit={output.ExitCode} stdout='{output.Stdout}'");
    }

    private static async Task<int> RunCwdOverride(IExecService exec, string sandbox)
    {
        string commandLine;
        string[] args;
        if (OperatingSystem.IsWindows())
        {
            commandLine = "cmd";
            args = ["/c", "cd"];
        }
        else
        {
            commandLine = "/bin/sh";
            args = ["-c", "pwd"];
        }

        var output = await exec.GetExecOutputAsync(commandLine, args, new ExecOptions
        {
            Silent = true,
            Cwd = sandbox,
        });

        var actual = output.Stdout.Trim();
        var expected = Path.GetFullPath(sandbox).TrimEnd(Path.DirectorySeparatorChar);
        var actualNorm = actual.TrimEnd(Path.DirectorySeparatorChar);

        // On macOS, /tmp is symlinked to /private/tmp; tolerate either rendering.
        var matches = string.Equals(actualNorm, expected, StringComparison.OrdinalIgnoreCase)
            || actualNorm.EndsWith(expected, StringComparison.OrdinalIgnoreCase)
            || expected.EndsWith(actualNorm, StringComparison.OrdinalIgnoreCase);

        return output.ExitCode == 0 && matches
            ? Ok("cwd-override", actual)
            : Fail($"cwd-override unexpected: exit={output.ExitCode} actual='{actual}' expected='{expected}'");
    }

    private static int Ok(string @case, string detail = "")
    {
        Console.WriteLine($"[OK] {@case}{(detail.Length > 0 ? $" {detail}" : string.Empty)}");
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine($"[FAIL] {message}");
        return 1;
    }
}
