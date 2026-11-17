// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using ActionsToolkit.Core;

namespace ActionsToolkit.Scripting.Tests;

public sealed class CoreTests
{
    [Fact]
    public void InputsRetainRequiredBooleanAndMultilineSemanticsWithoutRuntimeCredentials()
    {
        using var environment = new EnvironmentScope(new()
        {
            ["INPUT_SCRIPT_NAME"] = "  demo  ",
            ["INPUT_SCRIPT_BOOL"] = "true",
            ["INPUT_SCRIPT_LINES"] = "one\n\n two \n",
            ["INPUT_SCRIPT_MISSING"] = null,
            ["ACTIONS_RUNTIME_TOKEN"] = "not-a-jwt",
            ["GITHUB_EVENT_PATH"] = "missing-event.json",
        });

        Assert.Equal("demo", Core.GetInput("script_name", required: true));
        Assert.True(Core.GetBoolInput("script_bool", required: true));
        Assert.Equal(["one", "two"], Core.GetMultilineInput("script_lines", required: true));
        Assert.Empty(Core.GetInput("script_missing"));
        Assert.Throws<InvalidOperationException>(() => Core.GetInput("script_missing", required: true));
        Assert.Throws<InvalidOperationException>(() => Core.GetBoolInput("script_missing", required: true));
        Assert.Throws<InvalidOperationException>(() => Core.GetMultilineInput("script_missing", required: true));
    }

    [Fact]
    public async Task OutputsUseFileCommandsAndExplicitJsonMetadata()
    {
        using var directory = new TestDirectory();
        var path = directory.CreateFile("outputs");
        using var environment = new EnvironmentScope(new() { ["GITHUB_OUTPUT"] = path });
        var report = new Report("script", 2);

        await Core.SetOutputAsync("text", "first\nsecond");
        await Core.SetOutputAsync("ready", true);
        await Core.SetOutputAsync("missing", false);
        await Core.SetOutputAsync("report", report, TestJsonContext.Default.Report);
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await Core.SetOutputAsync("invalid", report, (JsonTypeInfo<Report>)null!));

        var text = await File.ReadAllTextAsync(path);
        Assert.Contains("text<<ghadelimiter_", text, StringComparison.Ordinal);
        Assert.Contains($"{Environment.NewLine}first\nsecond{Environment.NewLine}", text, StringComparison.Ordinal);
        Assert.Contains($"{Environment.NewLine}true{Environment.NewLine}", text, StringComparison.Ordinal);
        Assert.Contains($"{Environment.NewLine}false{Environment.NewLine}", text, StringComparison.Ordinal);
        Assert.Contains(JsonSerializer.Serialize(report, TestJsonContext.Default.Report), text, StringComparison.Ordinal);
        Assert.DoesNotContain("Report {", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NumericOutputsUseInvariantCultureForIntegersAndDecimals()
    {
        using var directory = new TestDirectory();
        var path = directory.CreateFile("numeric-outputs");
        using var environment = new EnvironmentScope(new() { ["GITHUB_OUTPUT"] = path });
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            await Core.SetOutputAsync("count", 42);
            await Core.SetOutputAsync("bytes", long.MaxValue);
            await Core.SetOutputAsync("decimal", 1234.5m);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }

        var text = await File.ReadAllTextAsync(path);
        Assert.Contains($"{Environment.NewLine}42{Environment.NewLine}", text, StringComparison.Ordinal);
        Assert.Contains($"{Environment.NewLine}9223372036854775807{Environment.NewLine}", text, StringComparison.Ordinal);
        Assert.Contains($"{Environment.NewLine}1234.5{Environment.NewLine}", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EnvironmentPathAndStateForwardFileCommands()
    {
        using var directory = new TestDirectory();
        var envFile = directory.CreateFile("env");
        var pathFile = directory.CreateFile("path");
        var stateFile = directory.CreateFile("state");
        using var environment = new EnvironmentScope(new()
        {
            ["GITHUB_ENV"] = envFile,
            ["GITHUB_PATH"] = pathFile,
            ["GITHUB_STATE"] = stateFile,
            ["SCRIPT_FACADE_TEST"] = null,
            ["STATE_script_state"] = "saved",
            ["PATH"] = Environment.GetEnvironmentVariable("PATH"),
        });

        await Core.ExportVariableAsync("SCRIPT_FACADE_TEST", "exported");
        await Core.AddPathAsync(directory.Root);
        await Core.SaveStateAsync("script_state", "saved");

        Assert.Equal("exported", Environment.GetEnvironmentVariable("SCRIPT_FACADE_TEST"));
        Assert.Contains("SCRIPT_FACADE_TEST<<", await File.ReadAllTextAsync(envFile), StringComparison.Ordinal);
        Assert.Contains(directory.Root, await File.ReadAllTextAsync(pathFile), StringComparison.Ordinal);
        Assert.StartsWith(directory.Root + Path.PathSeparator, Environment.GetEnvironmentVariable("PATH"), StringComparison.Ordinal);
        Assert.Equal("saved", Core.GetState("script_state"));
        Assert.Contains("script_state<<", await File.ReadAllTextAsync(stateFile), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DiagnosticsEscapeCommandsAndGroupsCloseAfterFailure()
    {
        using var output = new StringWriter();
        var previous = Console.Out;
        var exitCode = Environment.ExitCode;
        try
        {
            Console.SetOut(output);
            Core.SetSecret("secret\nline");
            Core.Info("info");
            Core.Debug("debug");
            Core.Warning("warning");
            Core.Error("error");
            Core.Notice("notice", new AnnotationProperties());
            Core.SetFailed("failed");
            Assert.NotEqual(0, Environment.ExitCode);
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await Core.GroupAsync("group", () => ValueTask.FromException(new InvalidOperationException("expected"))));
        }
        finally
        {
            Console.SetOut(previous);
            Environment.ExitCode = exitCode;
        }

        var text = output.ToString();
        Assert.Contains("::add-mask::secret%0Aline", text, StringComparison.Ordinal);
        Assert.Contains("::notice", text, StringComparison.Ordinal);
        Assert.Contains("::group::group", text, StringComparison.Ordinal);
        Assert.EndsWith("::endgroup::" + Environment.NewLine, text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SummaryRetainsItsBufferUntilExplicitWrite()
    {
        using var directory = new TestDirectory();
        var path = directory.CreateFile("summary");
        using var environment = new EnvironmentScope(new() { ["GITHUB_STEP_SUMMARY"] = path });

        var summary = Core.Summary.AddRaw("first");
        Core.Summary.AddRaw("second");
        Assert.Same(summary, Core.Summary);
        Assert.Equal("", await File.ReadAllTextAsync(path));
        await Core.Summary.WriteAsync();
        Assert.Contains("firstsecond", await File.ReadAllTextAsync(path), StringComparison.Ordinal);
        Assert.True(Core.Summary.IsBufferEmpty);
    }
}
