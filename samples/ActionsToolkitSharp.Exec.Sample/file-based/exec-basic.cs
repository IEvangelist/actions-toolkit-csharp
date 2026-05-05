// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// exec-basic.cs
//
// Mirrors the upstream `@actions/exec` README "Basic" snippet:
// https://github.com/actions/toolkit/tree/main/packages/exec#basic
//
// Runs a child process and streams its output to the live console. Equivalent to:
//   import * as exec from '@actions/exec';
//   await exec.exec('node', ['-v']);
//
// Run with:
//   dotnet run exec-basic.cs

#:package ActionsToolkitSharp.Exec@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Exec;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsExec()
    .BuildServiceProvider();

var exec = provider.GetRequiredService<IExecService>();

// Stream the running .NET SDK version to the live console.
var exitCode = await exec.ExecAsync("dotnet", ["--version"]);

Console.WriteLine($"Exit code: {exitCode}");
