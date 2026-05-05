// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// with-cwd.cs
//
// Mirrors the upstream `@actions/exec` README pattern of running a command
// in a different working directory via `options.cwd`:
// https://github.com/actions/toolkit/tree/main/packages/exec#exec-arguments
//
// Equivalent to:
//   await exec.exec('ls', [], { cwd: '/some/dir' });
//
// Run with:
//   dotnet run with-cwd.cs

#:package ActionsToolkit.Exec@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.Exec;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsExec()
    .BuildServiceProvider();

var exec = provider.GetRequiredService<IExecService>();

var sandbox = Path.Combine(Path.GetTempPath(), "actions-toolkit-sharp-exec-sample-cwd");
Directory.CreateDirectory(sandbox);

string commandLine;
string[] commandArgs;
if (OperatingSystem.IsWindows())
{
    commandLine = "cmd";
    commandArgs = ["/c", "cd"];
}
else
{
    commandLine = "/bin/sh";
    commandArgs = ["-c", "pwd"];
}

var output = await exec.GetExecOutputAsync(commandLine, commandArgs, new ExecOptions
{
    Silent = true,
    Cwd = sandbox,
});

Console.WriteLine($"Exit code : {output.ExitCode}");
Console.WriteLine($"Cwd seen  : {output.Stdout.Trim()}");
Console.WriteLine($"Sandbox   : {sandbox}");
