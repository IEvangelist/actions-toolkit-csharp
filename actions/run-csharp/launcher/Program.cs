// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ActionsToolkit.Runner;

internal static class Program
{
    private static async Task<int> Main()
    {
        using var cancellation = new CancellationTokenSource();
        var cancellationExitCode = 130;
        void Cancel(int exitCode)
        {
            Interlocked.Exchange(ref cancellationExitCode, exitCode);
            cancellation.Cancel();
        }

        using var terminate = OperatingSystem.IsLinux()
            ? PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
            {
                context.Cancel = true;
                Cancel(143);
            })
            : null;
        using var interrupt = OperatingSystem.IsLinux()
            ? PosixSignalRegistration.Create(PosixSignal.SIGINT, context =>
            {
                context.Cancel = true;
                Cancel(130);
            })
            : null;
        ConsoleCancelEventHandler handler = (_, args) =>
        {
            args.Cancel = true;
            Cancel(130);
        };
        if (!OperatingSystem.IsLinux())
        {
            Console.CancelKeyPress += handler;
        }

        try
        {
            return await RunAsync(ReadEnvironment(), Console.Out, Console.Error, cancellation.Token,
                () => Volatile.Read(ref cancellationExitCode));
        }
        finally
        {
            if (!OperatingSystem.IsLinux())
            {
                Console.CancelKeyPress -= handler;
            }
        }
    }

    internal static Dictionary<string, string?> ReadEnvironment() =>
        Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => (string?)entry.Value,
                OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

    internal static async Task<int> RunAsync(
        IReadOnlyDictionary<string, string?> environment,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken = default,
        Func<int>? cancellationExitCode = null,
        string executable = "dotnet")
    {
        try
        {
            var options = LauncherOptions.Parse(environment);
            if (!string.IsNullOrEmpty(options.Token))
            {
                await output.WriteLineAsync($"::add-mask::{EscapeCommand(options.Token)}");
                await output.FlushAsync(cancellationToken);
            }

            var startInfo = CreateStartInfo(options, environment, executable);
            return await RunProcessAsync(startInfo, cancellationToken, cancellationExitCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return cancellationExitCode?.Invoke() ?? 130;
        }
        catch (Exception exception) when (exception is ArgumentException or JsonException or IOException or
                                           UnauthorizedAccessException or Win32Exception or InvalidOperationException)
        {
            // Do not echo supplied JSON, paths, environment values or OS exception messages containing secrets.
            await error.WriteLineAsync("::error::C# action could not validate or launch the script. Check the workspace-relative .cs path, JSON inputs, .NET SDK availability and file permissions.");
            return 1;
        }
    }

    internal static ProcessStartInfo CreateStartInfo(
        LauncherOptions options,
        IReadOnlyDictionary<string, string?> environment,
        string executable = "dotnet")
    {
        var startInfo = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            WorkingDirectory = options.Workspace
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--file");
        startInfo.ArgumentList.Add(options.Script);
        startInfo.ArgumentList.Add("--");
        foreach (var argument in options.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.Environment.Clear();
        foreach (var (key, value) in environment)
        {
            if (!LauncherOptions.ControlVariables.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                startInfo.Environment[key] = value;
            }
        }

        foreach (var (key, value) in options.Inputs)
        {
            startInfo.Environment[key] = value;
        }

        if (!string.IsNullOrEmpty(options.Token))
        {
            startInfo.Environment["GITHUB_TOKEN"] = options.Token;
        }

        return startInfo;
    }

    internal static async Task<int> RunProcessAsync(
        ProcessStartInfo startInfo,
        CancellationToken cancellationToken,
        Func<int>? cancellationExitCode = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            throw new InvalidOperationException("The child process did not start.");
        }

        try
        {
            await process.WaitForExitAsync(cancellationToken);
            return cancellationToken.IsCancellationRequested ? cancellationExitCode?.Invoke() ?? 130 : process.ExitCode;
        }
        finally
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException) when (process.HasExited)
                {
                    // The child can exit between HasExited and Kill.
                }

                using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try
                {
                    await process.WaitForExitAsync(cleanup.Token);
                }
                catch (OperationCanceledException exception) when (cleanup.IsCancellationRequested)
                {
                    throw new InvalidOperationException("The child did not exit after cancellation.", exception);
                }
            }
        }
    }

    internal static string EscapeCommand(string value) =>
        value.Replace("%", "%25", StringComparison.Ordinal)
            .Replace("\r", "%0D", StringComparison.Ordinal)
            .Replace("\n", "%0A", StringComparison.Ordinal);
}
