// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// with-env.cs
//
// Mirrors the upstream `@actions/exec` README pattern of passing a custom
// environment to the child process via `options.env`:
// https://github.com/actions/toolkit/tree/main/packages/exec#exec-arguments
//
// Equivalent to:
//   await exec.exec('node', ['-e', 'console.log(process.env.MY_VAR)'],
//                   { env: { MY_VAR: 'hello' } });
//
// Run with:
//   dotnet run with-env.cs

#:package ActionsToolkitSharp.Exec@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Exec;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsExec()
    .BuildServiceProvider();

var exec = provider.GetRequiredService<IExecService>();

var env = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["ATS_EXEC_SAMPLE_VAR"] = "hello-from-exec-sample",
};

string commandLine;
string[] args;
if (OperatingSystem.IsWindows())
{
    commandLine = "cmd";
    args = ["/c", "echo %ATS_EXEC_SAMPLE_VAR%"];
}
else
{
    commandLine = "/bin/sh";
    args = ["-c", "printf '%s\\n' \"$ATS_EXEC_SAMPLE_VAR\""];
}

var output = await exec.GetExecOutputAsync(commandLine, args, new ExecOptions
{
    Silent = true,
    Env = env,
});

Console.WriteLine($"Exit code : {output.ExitCode}");
Console.WriteLine($"Stdout    : {output.Stdout.Trim()}");
