// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ActionsToolkitSharp.AotTestSupport;

/// <summary>
/// xUnit collection fixture that publishes the per-package <c>AotConsumer</c> project
/// to a native binary exactly once per test run. Tests then invoke the binary as a
/// subprocess to assert that each public API surface of the package is reachable
/// after Native AOT trimming and analysis.
/// </summary>
/// <remarks>
/// <para>
/// The fixture discovers the consumer csproj and assembly name via two
/// <c>[AssemblyMetadata]</c> attributes that each driver test assembly must declare:
/// <list type="bullet">
///   <item><c>AotConsumerProject</c> — absolute path to the consumer csproj.</item>
///   <item><c>AotConsumerExecutable</c> — the consumer's <c>AssemblyName</c>
///   (without extension).</item>
/// </list>
/// </para>
/// <para>
/// xUnit 2.x has no first-class dynamic skip API, so tests instead consult
/// <see cref="PublishSucceeded"/> and return early (with a console log) when the
/// publish step fails on the host machine. CI runners (Linux) are expected to
/// successfully publish; local Windows machines without the Native AOT workload may
/// not. The publish stdout/stderr are surfaced via <see cref="PublishStdout"/> and
/// <see cref="PublishStderr"/> for diagnostic purposes.
/// </para>
/// </remarks>
public sealed class AotPublishFixture : IDisposable
{
    // Trim/AOT analyzer warning code prefixes that we treat as failures.
    // See https://learn.microsoft.com/dotnet/core/deploying/native-aot/diagnostic-ids
    private static readonly Regex s_warningRegex = new(
        @"\b(IL2\d{3}|IL3\d{3})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly string _publishDir;

    public AotPublishFixture()
    {
        var assembly = typeof(AotPublishFixture).Assembly;
        var meta = assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToDictionary(static a => a.Key, static a => a.Value, StringComparer.Ordinal);

        if (!meta.TryGetValue("AotConsumerProject", out var consumerProject) ||
            string.IsNullOrWhiteSpace(consumerProject) ||
            !File.Exists(consumerProject))
        {
            PublishSucceeded = false;
            PublishError = $"AotConsumerProject metadata missing or path not found: '{consumerProject}'";
            _publishDir = string.Empty;
            return;
        }

        if (!meta.TryGetValue("AotConsumerExecutable", out var executableName) ||
            string.IsNullOrWhiteSpace(executableName))
        {
            PublishSucceeded = false;
            PublishError = "AotConsumerExecutable metadata missing.";
            _publishDir = string.Empty;
            return;
        }

        ConsumerProjectPath = consumerProject;
        ExecutableName = executableName;
        Rid = RuntimeInformation.RuntimeIdentifier;

        _publishDir = Path.Combine(
            Path.GetTempPath(),
            $"ats-aot-{executableName}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_publishDir);

        var binFileName = OperatingSystem.IsWindows()
            ? $"{executableName}.exe"
            : executableName;
        NativeBinaryPath = Path.Combine(_publishDir, binFileName);

        var (exitCode, stdout, stderr) = RunPublish(consumerProject, _publishDir, Rid);
        PublishStdout = stdout;
        PublishStderr = stderr;

        if (exitCode != 0)
        {
            PublishSucceeded = false;
            PublishError = $"dotnet publish failed (exit {exitCode}). See PublishStderr.";
            return;
        }

        if (!File.Exists(NativeBinaryPath))
        {
            PublishSucceeded = false;
            PublishError = $"Native binary missing at expected path: {NativeBinaryPath}";
            return;
        }

        var combined = stdout + Environment.NewLine + stderr;
        var matches = s_warningRegex.Matches(combined);
        if (matches.Count > 0)
        {
            var codes = matches.Cast<Match>()
                .Select(m => m.Value)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            PublishSucceeded = false;
            PublishError = $"Native AOT trim/analysis warnings detected: {string.Join(",", codes)}";
            return;
        }

        PublishSucceeded = true;
    }

    public bool PublishSucceeded { get; }
    public string? PublishError { get; }
    public string? NativeBinaryPath { get; }
    public string? ConsumerProjectPath { get; }
    public string? ExecutableName { get; }
    public string? Rid { get; }
    public string? PublishStdout { get; }
    public string? PublishStderr { get; }

    /// <summary>
    /// Runs the previously-published native binary with the supplied dispatcher case
    /// and arguments, returning the captured exit code, stdout, and stderr.
    /// </summary>
    public AotRunResult Run(string @case, params string[] extraArgs)
    {
        if (!PublishSucceeded || NativeBinaryPath is null)
        {
            throw new InvalidOperationException(
                "AotPublishFixture did not publish successfully; cannot run binary.");
        }

        var psi = new ProcessStartInfo(NativeBinaryPath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add(@case);
        foreach (var a in extraArgs)
        {
            psi.ArgumentList.Add(a);
        }

        // Provide a fake token so cases that need GITHUB_TOKEN do not fail.
        psi.Environment["GITHUB_TOKEN"] = "fake-aot-token";

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start: {NativeBinaryPath}");

        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();
        proc.WaitForExit(TimeSpan.FromSeconds(60));
        var stdout = stdoutTask.GetAwaiter().GetResult();
        var stderr = stderrTask.GetAwaiter().GetResult();

        return new AotRunResult(proc.ExitCode, stdout, stderr);
    }

    private static (int ExitCode, string Stdout, string Stderr) RunPublish(
        string projectPath,
        string outputDir,
        string rid)
    {
        var psi = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("publish");
        psi.ArgumentList.Add(projectPath);
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add("Release");
        psi.ArgumentList.Add("-r");
        psi.ArgumentList.Add(rid);
        psi.ArgumentList.Add("--self-contained");
        psi.ArgumentList.Add("true");
        psi.ArgumentList.Add("--nologo");
        psi.ArgumentList.Add("--verbosity");
        psi.ArgumentList.Add("minimal");
        psi.ArgumentList.Add("-o");
        psi.ArgumentList.Add(outputDir);

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        using var proc = new Process { StartInfo = psi };
        proc.OutputDataReceived += (_, e) => { if (e.Data is not null) stdout.AppendLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };
        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        // AOT publishes can be slow on cold caches. Allow up to 10 minutes.
        if (!proc.WaitForExit((int)TimeSpan.FromMinutes(10).TotalMilliseconds))
        {
            try { proc.Kill(entireProcessTree: true); } catch { /* ignore */ }
            return (-1, stdout.ToString(), stderr.ToString() + "[publish timed out]");
        }
        proc.WaitForExit();

        return (proc.ExitCode, stdout.ToString(), stderr.ToString());
    }

    public void Dispose()
    {
        if (!string.IsNullOrEmpty(_publishDir) && Directory.Exists(_publishDir))
        {
            try { Directory.Delete(_publishDir, recursive: true); }
            catch { /* best effort */ }
        }
    }
}

/// <summary>
/// The captured result of running an Aot consumer dispatcher case.
/// </summary>
public readonly record struct AotRunResult(int ExitCode, string Stdout, string Stderr);
