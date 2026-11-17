// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using ActionsToolkit.Runner;

namespace ActionsToolkit.Runner.Tests;

public sealed class LauncherTests : IDisposable
{
    private readonly string _root = Path.Combine(Directory.GetCurrentDirectory(), $".runner-tests-{Guid.NewGuid():N}");
    private readonly string _workspace;

    public LauncherTests()
    {
        _workspace = Path.Combine(_root, "workspace");
        Directory.CreateDirectory(_workspace);
        File.WriteAllText(Path.Combine(_workspace, "Directory.Build.props"), "<Project />");
        File.WriteAllText(Path.Combine(_workspace, "Directory.Build.targets"), "<Project />");
        File.WriteAllText(Path.Combine(_workspace, "Directory.Packages.props"), "<Project />");
        File.WriteAllText(Path.Combine(_workspace, "script.cs"), """
            using System.Diagnostics;
            using System.Text.Json;

            if (args is ["--descendant"])
            {
                await Task.Delay(TimeSpan.FromMinutes(2));
                return 0;
            }

            if (args is ["--wait"])
            {
                var startInfo = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false };
                if (Path.GetFileNameWithoutExtension(Environment.ProcessPath!) == "dotnet")
                {
                    startInfo.ArgumentList.Add(Environment.GetCommandLineArgs()[0]);
                }
                startInfo.ArgumentList.Add("--descendant");
                using var descendant = Process.Start(startInfo)!;
                File.WriteAllText(Environment.GetEnvironmentVariable("TEST_READY")!,
                    $"{Environment.ProcessId},{descendant.Id}");
                await Task.Delay(TimeSpan.FromMinutes(2));
                return 0;
            }

            using (var stream = File.Create(Environment.GetEnvironmentVariable("TEST_RESULT")!))
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteStartArray("Args");
                foreach (var argument in args)
                {
                    writer.WriteStringValue(argument);
                }
                writer.WriteEndArray();
                writer.WriteStartObject("Environment");
                foreach (var key in new[] { "INPUT_MY_INPUT", "GITHUB_TOKEN", "ACTIONS_RUNTIME_TOKEN",
                    "INPUT_SCRIPT", "INPUT_ARGS", "INPUT_INPUTS", "INPUT_GITHUB-TOKEN", "INPUT_GITHUB_TOKEN",
                    "ACTIONS_TOOLKIT_SCRIPT", "ACTIONS_TOOLKIT_ARGS", "ACTIONS_TOOLKIT_INPUTS", "ACTIONS_TOOLKIT_GITHUB_TOKEN" })
                {
                    if (Environment.GetEnvironmentVariable(key) is { } value)
                    {
                        writer.WriteString(key, value);
                    }
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            File.AppendAllText(Environment.GetEnvironmentVariable("GITHUB_OUTPUT")!, "result=works\n");
            File.AppendAllText(Environment.GetEnvironmentVariable("GITHUB_ENV")!, "FROM_SCRIPT=yes\n");
            return int.Parse(Environment.GetEnvironmentVariable("TEST_EXIT_CODE") ?? "0");
            """);
    }

    [Theory]
    [InlineData("")]
    [InlineData("../outside.cs")]
    [InlineData("missing.cs")]
    [InlineData("script.txt")]
    [InlineData("script.cs\0")]
    [InlineData("/script.cs")]
    [InlineData("C:\\script.cs")]
    [InlineData("script.cs/other.cs")]
    public async Task InvalidPathsFailBeforeSpawnAsync(string script)
    {
        var environment = CreateEnvironment();
        environment[LauncherOptions.ScriptVariable] = script;
        await AssertValidationFailureAsync(environment);
    }

    [Fact]
    public async Task MissingWorkspaceFailsBeforeSpawnAsync()
    {
        var environment = CreateEnvironment();
        environment.Remove("GITHUB_WORKSPACE");
        await AssertValidationFailureAsync(environment);
    }

    [Fact]
    public async Task SiblingWorkspaceWithSamePrefixIsNotContainedAsync()
    {
        var sibling = Path.Combine(_root, "workspace-sibling");
        Directory.CreateDirectory(sibling);
        File.WriteAllText(Path.Combine(sibling, "outside.cs"), "// outside");
        var environment = CreateEnvironment();
        environment[LauncherOptions.ScriptVariable] = "../workspace-sibling/outside.cs";

        await AssertValidationFailureAsync(environment);
    }

    [Theory]
    [InlineData(LauncherOptions.ArgumentsVariable, "{}")]
    [InlineData(LauncherOptions.ArgumentsVariable, "null")]
    [InlineData(LauncherOptions.ArgumentsVariable, "[true]")]
    [InlineData(LauncherOptions.ArgumentsVariable, "[1]")]
    [InlineData(LauncherOptions.ArgumentsVariable, "[null]")]
    [InlineData(LauncherOptions.ArgumentsVariable, "[\"\\u0000\"]")]
    [InlineData(LauncherOptions.ArgumentsVariable, "[")]
    [InlineData(LauncherOptions.InputsVariable, "[]")]
    [InlineData(LauncherOptions.InputsVariable, "null")]
    [InlineData(LauncherOptions.InputsVariable, "{\"hello\":4}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"hello\":null}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"hello\":{}}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"hello\":\"\\u0000\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"hello\":\"one\",\"HELLO\":\"two\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"hello world\":\"one\",\"hello_world\":\"two\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"hello\":\"one\",\"hello\":\"two\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"bad\\nname\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"bad=name\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"script\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"ARGS\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"inputs\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"github-token\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"github_token\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"github env\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"GITHUB_OUTPUT\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"ACTIONS_RUNTIME_TOKEN\":\"value\"}")]
    [InlineData(LauncherOptions.InputsVariable, "{\"runner_temp\":\"value\"}")]
    [InlineData(LauncherOptions.TokenVariable, "token\0")]
    public async Task InvalidJsonAndReservedInputsFailBeforeSpawnAsync(string key, string value)
    {
        var environment = CreateEnvironment();
        environment[key] = value;
        await AssertValidationFailureAsync(environment);
    }

    [Fact]
    public async Task SymlinksCannotEscapeWorkspaceAsync()
    {
        var outside = Path.Combine(_root, "outside");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "outside.cs"), "// outside");
        File.CreateSymbolicLink(Path.Combine(_workspace, "escape.cs"), Path.Combine(outside, "outside.cs"));
        Directory.CreateSymbolicLink(Path.Combine(_workspace, "escape-directory"), outside);
        File.CreateSymbolicLink(Path.Combine(_workspace, "indirect.cs"),
            Path.Combine(_workspace, "escape-directory", "outside.cs"));
        Directory.CreateSymbolicLink(Path.Combine(_workspace, "ancestor"), _root);

        foreach (var script in new[] { "escape.cs", "escape-directory/outside.cs", "indirect.cs",
            "ancestor/workspace/script.cs" })
        {
            var environment = CreateEnvironment();
            environment[LauncherOptions.ScriptVariable] = script;
            await AssertValidationFailureAsync(environment);
        }
    }

    [Fact]
    public void InternalSymlinksAndSymlinkedWorkspaceResolveToRealPaths()
    {
        var alias = Path.Combine(_root, "workspace-alias");
        Directory.CreateSymbolicLink(alias, _workspace);
        File.CreateSymbolicLink(Path.Combine(_workspace, "alias.cs"), Path.Combine(_workspace, "script.cs"));
        var environment = CreateEnvironment();
        environment["GITHUB_WORKSPACE"] = alias;
        environment[LauncherOptions.ScriptVariable] = "alias.cs";

        var options = LauncherOptions.Parse(environment);

        Assert.Equal(_workspace, options.Workspace);
        Assert.Equal(Path.Combine(_workspace, "script.cs"), options.Script);
    }

    [Fact]
    public void StartInfoUsesLiteralArgumentsAndFiltersOnlyControlEnvironment()
    {
        var environment = CreateEnvironment();
        environment[LauncherOptions.ArgumentsVariable] = """["two words", "\"quoted\"", "", "; $(anything)", "--flag"]""";
        environment[LauncherOptions.InputsVariable] = """{"my input":"value\nsecond line","with-hyphen":""}""";
        environment[LauncherOptions.TokenVariable] = "explicit-token";
        environment["GITHUB_TOKEN"] = "inherited-token";
        environment["ACTIONS_RUNTIME_TOKEN"] = "runtime-credential";
        environment["ACTIONS_ID_TOKEN_REQUEST_TOKEN"] = "oidc-credential";
        environment["ACTIONS_CACHE_URL"] = "cache-url";
        environment["ACTIONS_RESULTS_URL"] = "results-url";
        environment["INPUT_SCRIPT"] = "leaked-script";
        environment["input_args"] = "leaked-args";
        environment["INPUT_INPUTS"] = "leaked-inputs";
        environment["INPUT_GITHUB-TOKEN"] = "leaked-token";
        environment["INPUT_GITHUB_TOKEN"] = "leaked-token";
        environment["INPUT_EXISTING"] = "existing-input";

        var options = LauncherOptions.Parse(environment);
        var startInfo = Program.CreateStartInfo(options, environment);

        Assert.False(startInfo.UseShellExecute);
        Assert.Equal(_workspace, startInfo.WorkingDirectory);
        Assert.Equal(["run", "--file", options.Script, "--", "two words", "\"quoted\"", "", "; $(anything)", "--flag"],
            startInfo.ArgumentList);
        Assert.Equal("", startInfo.Arguments);
        Assert.Equal("explicit-token", startInfo.Environment["GITHUB_TOKEN"]);
        Assert.Equal("value\nsecond line", startInfo.Environment["INPUT_MY_INPUT"]);
        Assert.Equal("", startInfo.Environment["INPUT_WITH-HYPHEN"]);
        Assert.Equal("existing-input", startInfo.Environment["INPUT_EXISTING"]);
        foreach (var control in LauncherOptions.ControlVariables)
        {
            Assert.DoesNotContain(startInfo.Environment.Keys, key => string.Equals(key, control, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var key in new[] { "GITHUB_ENV", "GITHUB_OUTPUT", "GITHUB_PATH", "GITHUB_STEP_SUMMARY",
            "GITHUB_STATE", "ACTIONS_RUNTIME_TOKEN", "ACTIONS_CACHE_URL", "ACTIONS_RESULTS_URL",
            "ACTIONS_ID_TOKEN_REQUEST_TOKEN", "ACTIONS_ID_TOKEN_REQUEST_URL" })
        {
            Assert.True(startInfo.Environment.TryGetValue(key, out var value) || !environment.ContainsKey(key));
            Assert.Equal(environment.GetValueOrDefault(key), value);
        }
    }

    [Fact]
    public void OmittedTokenPreservesInheritedTokenWithoutInventingOne()
    {
        var environment = CreateEnvironment();
        var options = LauncherOptions.Parse(environment);
        var withoutToken = Program.CreateStartInfo(options, environment);
        Assert.False(withoutToken.Environment.ContainsKey("GITHUB_TOKEN"));
        environment["GITHUB_TOKEN"] = "already-inherited";
        var inherited = Program.CreateStartInfo(options, environment);
        Assert.Equal("already-inherited", inherited.Environment["GITHUB_TOKEN"]);
    }

    [Fact]
    public async Task ExplicitTokenIsMaskedWithWorkflowCommandEscapingBeforeSpawnAsync()
    {
        var environment = CreateEnvironment();
        environment[LauncherOptions.TokenVariable] = "percent%\r\n::error::not-a-command";
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        using var error = new StringWriter(CultureInfo.InvariantCulture);

        var exitCode = await Program.RunAsync(environment, output, error, executable: Path.Combine(_root, "no-dotnet"));

        Assert.Equal(1, exitCode);
        Assert.Equal($"::add-mask::percent%25%0D%0A::error::not-a-command{Environment.NewLine}", output.ToString());
        Assert.DoesNotContain("percent%", error.ToString(), StringComparison.Ordinal);
        Assert.False(File.Exists(environment["GITHUB_ENV"]));
        Assert.False(File.Exists(environment["GITHUB_OUTPUT"]));
    }

    [Fact]
    public async Task CancellationBeforeStartDoesNotLaunchChildAsync()
    {
        var environment = CreateEnvironment();
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        using var error = new StringWriter(CultureInfo.InvariantCulture);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var exitCode = await Program.RunAsync(environment, output, error, cancellation.Token);

        Assert.Equal(130, exitCode);
        Assert.False(File.Exists(environment["TEST_RESULT"]));
        Assert.Equal("", error.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(23)]
    public async Task ActualFileAppPreservesArgumentsEnvironmentFileCommandsAndExitCodeAsync(int expectedExitCode)
    {
        var environment = CreateEnvironment();
        environment[LauncherOptions.ArgumentsVariable] = """["two words","\"quoted\"","","; $(echo nope)","--flag"]""";
        environment[LauncherOptions.InputsVariable] = """{"my input":"hello\nworld"}""";
        environment[LauncherOptions.TokenVariable] = "explicit-test-token";
        environment["ACTIONS_RUNTIME_TOKEN"] = "runtime-test-token";
        environment["INPUT_SCRIPT"] = "control-input";
        environment["TEST_EXIT_CODE"] = expectedExitCode.ToString(CultureInfo.InvariantCulture);
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        using var error = new StringWriter(CultureInfo.InvariantCulture);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));

        var exitCode = await Program.RunAsync(environment, output, error, timeout.Token);

        Assert.Equal("", error.ToString());
        Assert.Equal(expectedExitCode, exitCode);
        using var result = JsonDocument.Parse(await File.ReadAllTextAsync(environment["TEST_RESULT"]!, timeout.Token));
        var arguments = result.RootElement.GetProperty("Args").EnumerateArray().Select(value => value.GetString()!).ToArray();
        Assert.Equal(["two words", "\"quoted\"", "", "; $(echo nope)", "--flag"], arguments);
        var childEnvironment = result.RootElement.GetProperty("Environment");
        Assert.Equal("hello\nworld", childEnvironment.GetProperty("INPUT_MY_INPUT").GetString());
        Assert.Equal("explicit-test-token", childEnvironment.GetProperty("GITHUB_TOKEN").GetString());
        Assert.Equal("runtime-test-token", childEnvironment.GetProperty("ACTIONS_RUNTIME_TOKEN").GetString());
        foreach (var control in LauncherOptions.ControlVariables)
        {
            Assert.False(childEnvironment.TryGetProperty(control, out _));
        }

        Assert.Equal("result=works\n", await File.ReadAllTextAsync(environment["GITHUB_OUTPUT"]!, timeout.Token));
        Assert.Equal("FROM_SCRIPT=yes\n", await File.ReadAllTextAsync(environment["GITHUB_ENV"]!, timeout.Token));
    }

    [Fact]
    public async Task CancellationKillsActualChildAndDescendantAsync()
    {
        var environment = CreateEnvironment();
        environment[LauncherOptions.ArgumentsVariable] = """["--wait"]""";
        var ready = Path.Combine(_workspace, "ready");
        environment["TEST_READY"] = ready;
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        using var error = new StringWriter(CultureInfo.InvariantCulture);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var execution = Program.RunAsync(environment, output, error, cancellation.Token, () => 143);
        int[] processIds = [];
        try
        {
            using var readyTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            while (!File.Exists(ready))
            {
                Assert.False(execution.IsCompleted, error.ToString());
                await Task.Delay(100, readyTimeout.Token);
            }

            processIds = (await File.ReadAllTextAsync(ready, readyTimeout.Token))
                .Split(',').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
            await cancellation.CancelAsync();

            Assert.Equal(143, await execution.WaitAsync(TimeSpan.FromSeconds(15)));
            foreach (var processId in processIds)
            {
                Assert.False(IsRunning(processId), $"Process {processId} survived cancellation.");
            }
        }
        finally
        {
            await cancellation.CancelAsync();
            await execution.WaitAsync(TimeSpan.FromSeconds(15));
            foreach (var processId in processIds)
            {
                if (IsRunning(processId))
                {
                    using var process = Process.GetProcessById(processId);
                    process.Kill(entireProcessTree: true);
                }
            }
        }
    }

    private static bool IsRunning(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private Dictionary<string, string?> CreateEnvironment()
    {
        var environment = Program.ReadEnvironment();
        foreach (var key in environment.Keys.Where(key => key.StartsWith("GITHUB_", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("ACTIONS_", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("INPUT_", StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            environment.Remove(key);
        }

        environment["GITHUB_WORKSPACE"] = _workspace;
        environment[LauncherOptions.ScriptVariable] = "script.cs";
        environment["TEST_RESULT"] = Path.Combine(_workspace, "result.json");
        foreach (var key in new[] { "GITHUB_OUTPUT", "GITHUB_ENV", "GITHUB_PATH", "GITHUB_STEP_SUMMARY", "GITHUB_STATE" })
        {
            environment[key] = Path.Combine(_workspace, key);
        }

        return environment;
    }

    private static async Task AssertValidationFailureAsync(Dictionary<string, string?> environment)
    {
        Assert.ThrowsAny<Exception>(() => LauncherOptions.Parse(environment));
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        using var error = new StringWriter(CultureInfo.InvariantCulture);
        // A valid request would execute this real script and create result.json and file commands.
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        Assert.Equal(1, await Program.RunAsync(environment, output, error, timeout.Token));
        Assert.False(File.Exists(environment["TEST_RESULT"]));
        Assert.False(File.Exists(environment["GITHUB_OUTPUT"]));
        Assert.False(File.Exists(environment["GITHUB_ENV"]));
        Assert.StartsWith("::error::", error.ToString(), StringComparison.Ordinal);
        Assert.Equal("", output.ToString());
    }

    public void Dispose()
    {
        Directory.Delete(_root, recursive: true);
    }
}
