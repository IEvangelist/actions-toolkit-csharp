// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;

namespace ActionsToolkit.ToolCache;

/// <summary>
/// Resolves the current OS distribution version, mirroring upstream
/// <c>manifest._getOsVersion</c>:
/// <list type="bullet">
///   <item><c>linux</c>: parses <c>/etc/lsb-release</c> or <c>/etc/os-release</c>.</item>
///   <item><c>darwin</c>: shells out to <c>sw_vers -productVersion</c>.</item>
///   <item>other: returns an empty string.</item>
/// </list>
/// </summary>
internal static class OsVersionResolver
{
    /// <summary>
    /// Test seam: when set, overrides the distribution-version reader on
    /// Linux. Used by the manifest tests to mock <c>/etc/lsb-release</c>.
    /// </summary>
    internal static Func<string>? LinuxVersionFileOverride { get; set; }

    /// <summary>
    /// Test seam: when set, overrides the macOS <c>sw_vers</c> shell-out.
    /// </summary>
    internal static Func<string>? DarwinVersionOverride { get; set; }

    public static string GetOsVersion()
    {
        if (OperatingSystem.IsMacOS())
        {
            if (DarwinVersionOverride is { } macOverride)
            {
                return macOverride();
            }
            return ExecSwVers();
        }

        if (OperatingSystem.IsLinux())
        {
            var contents = LinuxVersionFileOverride is { } override_
                ? override_()
                : ReadLinuxVersionFile();
            if (string.IsNullOrEmpty(contents))
            {
                return string.Empty;
            }

            foreach (var rawLine in contents.Split('\n'))
            {
                var line = rawLine.Trim();
                var idx = line.IndexOf('=', StringComparison.Ordinal);
                if (idx <= 0) continue;
                var key = line[..idx].Trim();
                var value = line[(idx + 1)..].Trim();
                if (key is "VERSION_ID" or "DISTRIB_RELEASE")
                {
                    if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
                    {
                        value = value[1..^1];
                    }
                    return value;
                }
            }
        }

        return string.Empty;
    }

    internal static string ReadLinuxVersionFile()
    {
        const string lsb = "/etc/lsb-release";
        const string os = "/etc/os-release";
        try
        {
            if (File.Exists(lsb))
            {
                return File.ReadAllText(lsb);
            }
            if (File.Exists(os))
            {
                return File.ReadAllText(os);
            }
        }
        catch (IOException)
        {
        }
        return string.Empty;
    }

    private static string ExecSwVers()
    {
        try
        {
            var psi = new ProcessStartInfo("sw_vers")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("-productVersion");
            using var proc = Process.Start(psi);
            if (proc is null) return string.Empty;
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(5000);
            return output.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }
}
