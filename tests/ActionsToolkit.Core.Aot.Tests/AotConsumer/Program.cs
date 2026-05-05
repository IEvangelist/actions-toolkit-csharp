// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Core.Extensions;
using ActionsToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.Core.AotConsumer;

/// <summary>
/// Native AOT dispatcher that exercises the public surface of
/// <see cref="ActionsToolkit.Core"/>. Each <c>case</c> mirrors a concern from
/// the upstream <c>actions/toolkit/packages/core/__tests__/core.test.ts</c>,
/// <c>command.test.ts</c>, <c>file-command.test.ts</c>, and <c>summary.test.ts</c>
/// suites.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: actions-toolkit-core-aot-consumer <case>");
        }

        var sandbox = Path.Combine(Path.GetTempPath(), "actions-toolkit-core-aot-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(sandbox);

        try
        {
            // Pre-populate workflow-command file paths so that file-backed APIs do
            // not fall back to ::set-output style stdout commands. This keeps the
            // [OK] marker visible at the top of stdout for the test driver.
            ConfigureWorkflowFiles(sandbox);

            var services = new ServiceCollection()
                .AddGitHubActionsCore()
                .BuildServiceProvider();
            var core = services.GetRequiredService<ICoreService>();

            return args[0] switch
            {
                "info" => RunInfo(core),
                "warning" => RunWarning(core),
                "error" => RunError(core),
                "notice" => RunNotice(core),
                "debug" => RunDebug(core),
                "set-output" => RunSetOutput(core),
                "set-secret" => RunSetSecret(core),
                "add-path" => RunAddPath(core),
                "export-variable" => RunExportVariable(core),
                "get-input" => RunGetInput(core),
                "get-multiline-input" => RunGetMultilineInput(core),
                "get-bool-input" => RunGetBoolInput(core),
                "set-failed" => RunSetFailed(core),
                "summary" => RunSummary(core),
                "group" => RunGroup(core),
                "save-state" => RunSaveState(core),
                "get-state" => RunGetState(core),
                "command-echo" => RunCommandEcho(core),
                "is-debug" => RunIsDebug(core),
                _ => Fail($"Unknown case: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            return Fail($"{ex.GetType().FullName}: {ex.Message}");
        }
    }

    private static void ConfigureWorkflowFiles(string sandbox)
    {
        void Set(string key, string fileName)
        {
            var path = Path.Combine(sandbox, fileName);
            File.WriteAllText(path, string.Empty);
            Environment.SetEnvironmentVariable(key, path);
        }

        Set("GITHUB_ENV", "env");
        Set("GITHUB_PATH", "path");
        Set("GITHUB_OUTPUT", "output");
        Set("GITHUB_STATE", "state");
        Set("GITHUB_STEP_SUMMARY", "summary.md");
    }

    private static int RunInfo(ICoreService core)
    {
        core.WriteInfo("aot-info-message");
        return Ok("info");
    }

    private static int RunWarning(ICoreService core)
    {
        core.WriteWarning("aot-warning-message");
        return Ok("warning");
    }

    private static int RunError(ICoreService core)
    {
        core.WriteError("aot-error-message");
        return Ok("error");
    }

    private static int RunNotice(ICoreService core)
    {
        core.WriteNotice("aot-notice-message");
        return Ok("notice");
    }

    private static int RunDebug(ICoreService core)
    {
        core.WriteDebug("aot-debug-message");
        return Ok("debug");
    }

    private static int RunSetOutput(ICoreService core)
    {
        core.SetOutputAsync<string>("aot-key", "aot-value").AsTask().GetAwaiter().GetResult();
        return Ok("set-output");
    }

    private static int RunSetSecret(ICoreService core)
    {
        core.SetSecret("aot-secret");
        return Ok("set-secret");
    }

    private static int RunAddPath(ICoreService core)
    {
        var path = Path.Combine(Path.GetTempPath(), "aot-added-path");
        Directory.CreateDirectory(path);
        core.AddPathAsync(path).AsTask().GetAwaiter().GetResult();
        return Ok("add-path");
    }

    private static int RunExportVariable(ICoreService core)
    {
        core.ExportVariableAsync("AOT_VAR", "aot-value").AsTask().GetAwaiter().GetResult();
        var resolved = Environment.GetEnvironmentVariable("AOT_VAR") ?? string.Empty;
        return resolved == "aot-value"
            ? Ok("export-variable", resolved)
            : Fail($"export-variable did not set env var (got: '{resolved}').");
    }

    private static int RunGetInput(ICoreService core)
    {
        Environment.SetEnvironmentVariable("INPUT_AOT_NAME", "  hello  ");
        var trimmed = core.GetInput("aot_name");
        return trimmed == "hello"
            ? Ok("get-input", trimmed)
            : Fail($"get-input did not trim (got: '{trimmed}').");
    }

    private static int RunGetMultilineInput(ICoreService core)
    {
        Environment.SetEnvironmentVariable("INPUT_AOT_LINES", "one\ntwo\nthree");
        var lines = core.GetMultilineInput("aot_lines");
        return lines.Length == 3
            ? Ok("get-multiline-input", $"lines={lines.Length}")
            : Fail($"get-multiline-input wrong count: {lines.Length}");
    }

    private static int RunGetBoolInput(ICoreService core)
    {
        Environment.SetEnvironmentVariable("INPUT_AOT_BOOL", "true");
        var value = core.GetBoolInput("aot_bool");
        return value
            ? Ok("get-bool-input", "true")
            : Fail("get-bool-input did not parse true.");
    }

    private static int RunSetFailed(ICoreService core)
    {
        core.SetFailed("aot-failure-message");
        // SetFailed assigns Environment.ExitCode = 1; reset so the dispatcher
        // exits with 0 and the test asserts the API was reachable under AOT.
        Environment.ExitCode = 0;
        return Ok("set-failed");
    }

    private static int RunSummary(ICoreService core)
    {
        var summary = core.Summary;
        summary.AddRaw("# AOT", addNewLine: true);
        summary.AddRaw("- bullet", addNewLine: true);
        return summary.IsBufferEmpty
            ? Fail("summary buffer was empty after writes.")
            : Ok("summary", $"available={Summaries.Summary.IsAvailable}");
    }

    private static int RunGroup(ICoreService core)
    {
        core.StartGroup("aot-group");
        core.EndGroup();
        var result = core.GroupAsync<int>("aot-async-group", () => ValueTask.FromResult(42))
            .AsTask().GetAwaiter().GetResult();
        return result == 42
            ? Ok("group", $"result={result}")
            : Fail("group async returned wrong value.");
    }

    private static int RunSaveState(ICoreService core)
    {
        core.SaveStateAsync<string>("AOT_STATE", "state-value").AsTask().GetAwaiter().GetResult();
        return Ok("save-state");
    }

    private static int RunGetState(ICoreService core)
    {
        Environment.SetEnvironmentVariable("STATE_AOT_KEY", "state-cache");
        var value = core.GetState("AOT_KEY");
        return value == "state-cache"
            ? Ok("get-state", value)
            : Fail($"get-state returned '{value}'.");
    }

    private static int RunCommandEcho(ICoreService core)
    {
        core.SetCommandEcho(true);
        core.SetCommandEcho(false);
        return Ok("command-echo");
    }

    private static int RunIsDebug(ICoreService core)
    {
        var debug = core.IsDebug;
        return Ok("is-debug", debug ? "true" : "false");
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
