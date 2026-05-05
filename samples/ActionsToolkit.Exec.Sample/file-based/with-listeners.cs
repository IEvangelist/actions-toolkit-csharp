// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// with-listeners.cs
//
// Mirrors the upstream `@actions/exec` README "Output listeners" snippet:
// https://github.com/actions/toolkit/tree/main/packages/exec#output-listeners
//
// Wires per-line / per-chunk callbacks via `ExecListeners` so the host
// process can react to output as it streams. Equivalent to:
//   const options = { listeners: { stdline: l => myBuf += l + '\n' } };
//   await exec.exec('node', ['-v'], options);
//
// Run with:
//   dotnet run with-listeners.cs

#:package ActionsToolkit.Exec@*
#:package Microsoft.Extensions.DependencyInjection@*

using System.Text;

using ActionsToolkit.Exec;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsExec()
    .BuildServiceProvider();

var exec = provider.GetRequiredService<IExecService>();

var perLine = new List<string>();
var byteCount = 0;

var options = new ExecOptions
{
    Silent = true,
    Listeners = new ExecListeners
    {
        Stdline = line => perLine.Add(line),
        Stdout = chunk => byteCount += chunk.Length,
    },
};

var exitCode = await exec.ExecAsync("dotnet", ["--info"], options);

Console.WriteLine($"Exit code     : {exitCode}");
Console.WriteLine($"Lines captured: {perLine.Count}");
Console.WriteLine($"Bytes captured: {byteCount}");
Console.WriteLine($"First line    : {perLine.FirstOrDefault()}");
