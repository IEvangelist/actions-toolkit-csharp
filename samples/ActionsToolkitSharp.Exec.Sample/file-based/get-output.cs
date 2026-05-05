// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// get-output.cs
//
// Mirrors the upstream `@actions/exec` README "getExecOutput" snippet:
// https://github.com/actions/toolkit/tree/main/packages/exec#getexecoutput
//
// Runs a child process and captures its stdout/stderr in addition to streaming
// to the live console. Equivalent to:
//   const { exitCode, stdout, stderr } = await exec.getExecOutput('git', ['status']);
//
// Run with:
//   dotnet run get-output.cs

#:package ActionsToolkitSharp.Exec@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Exec;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsExec()
    .BuildServiceProvider();

var exec = provider.GetRequiredService<IExecService>();

var output = await exec.GetExecOutputAsync(
    "dotnet",
    ["--list-sdks"],
    new ExecOptions { Silent = true });

Console.WriteLine($"Exit code : {output.ExitCode}");
Console.WriteLine($"Stdout    : {output.Stdout.Trim()}");
Console.WriteLine($"Stderr    : {output.Stderr.Trim()}");
