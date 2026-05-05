// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Exec.Tests;

/// <summary>
/// Cross-platform helpers used by the upstream-traceable tests in this assembly.
/// </summary>
internal static class TestPlatform
{
    public static bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Builds a portable, single-line <c>echo</c>-style invocation for the current
    /// platform that prints <paramref name="message"/> on stdout and returns 0.
    /// On Windows, uses <c>cmd /c echo &lt;msg&gt;</c>; on Unix, uses
    /// <c>/bin/sh -c "printf '&lt;msg&gt;\n'"</c>.
    /// </summary>
    public static (string CommandLine, string[] Args) EchoCommand(string message)
    {
        if (IsWindows)
        {
            return ("cmd", ["/c", "echo", message]);
        }

        return ("/bin/sh", ["-c", $"printf '%s\\n' '{message}'"]);
    }

    /// <summary>
    /// A portable, fast-exiting tool that emits at least one stderr line and exits 0.
    /// </summary>
    public static (string CommandLine, string[] Args) StderrCommand(string message)
    {
        if (IsWindows)
        {
            // 1>&2 redirects echo output to stderr.
            return ("cmd", ["/c", $"echo {message} 1>&2"]);
        }

        return ("/bin/sh", ["-c", $"printf '%s' '{message}' 1>&2"]);
    }

    /// <summary>
    /// A portable, non-zero-exiting tool useful for ignore-return-code / failure tests.
    /// </summary>
    public static (string CommandLine, string[] Args) FailingCommand(int code)
    {
        if (IsWindows)
        {
            return ("cmd", ["/c", $"exit {code}"]);
        }

        return ("/bin/sh", ["-c", $"exit {code}"]);
    }

    /// <summary>
    /// A portable command that reads stdin and echoes it back to stdout.
    /// </summary>
    public static (string CommandLine, string[] Args) CatStdinCommand()
    {
        if (IsWindows)
        {
            // findstr /N:^ matches every line and prints with line numbers we don't want;
            // the more idiomatic Windows passthrough is `more`, but it requires a TTY.
            // Use PowerShell to read stdin and write it back.
            return ("powershell", ["-NoProfile", "-Command", "[Console]::In.ReadToEnd() | Write-Host -NoNewline"]);
        }

        return ("/bin/sh", ["-c", "cat"]);
    }
}
