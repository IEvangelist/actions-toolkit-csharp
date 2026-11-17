#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using System.Globalization;
using ActionsToolkit.Exec;

var result = await Exec.GetOutputAsync("dotnet", ["--version"]);
await Exec.RunAsync("dotnet", ["build", "path-tool.cs", "-o", "process-build", "--nologo"]);
if (!File.Exists(Path.Combine("process-build", "path-tool.dll")))
    throw new InvalidOperationException("The child build did not produce the expected assembly.");
await Core.SetOutputAsync("sdk", result.Stdout.Trim());
var failed = await Exec.GetOutputAsync("dotnet", ["--definitely-not-a-command"],
    new ExecOptions { IgnoreReturnCode = true, Silent = true });
if (failed.ExitCode == 0)
    throw new InvalidOperationException("Expected the negative process probe to fail.");
await Core.SetOutputAsync("negativeExit", failed.ExitCode.ToString(CultureInfo.InvariantCulture));
await Core.Summary.AddMarkdownHeading("Typed process results").AddNewLine()
    .AddRawMarkdown($"SDK `{result.Stdout.Trim()}`; controlled negative command returned `{failed.ExitCode}`.", true).WriteAsync();
