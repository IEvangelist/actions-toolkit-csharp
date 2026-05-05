# `ActionsToolkitSharp.Exec` package

To install the [`ActionsToolkitSharp.Exec`](https://www.nuget.org/packages/ActionsToolkitSharp.Exec) NuGet package:

```xml
<PackageReference Include="ActionsToolkitSharp.Exec" Version="[Version]" />
```

Or use the [`dotnet add package`](https://learn.microsoft.com/dotnet/core/tools/dotnet-add-package) .NET CLI command:

```bash
dotnet add package ActionsToolkitSharp.Exec
```

## Get the `IExecService` instance

`IExecService` mirrors the upstream [`@actions/exec`](https://github.com/actions/toolkit/tree/main/packages/exec) Node.js API. Register it with the DI container by calling `AddGitHubActionsExec`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using ActionsToolkitSharp.Exec;

using var provider = new ServiceCollection()
    .AddGitHubActionsExec()
    .BuildServiceProvider();

var exec = provider.GetRequiredService<IExecService>();
```

## `ExecAsync` — run a tool, get its exit code

```csharp
// Run a tool with no extra args.
int code = await exec.ExecAsync("dotnet --version");

// Pass args separately. Escaping is handled by the runner.
code = await exec.ExecAsync("dotnet", ["--info"]);
```

By default `ExecAsync` throws `InvalidOperationException` when the child returns a non-zero exit code. Set `IgnoreReturnCode = true` to receive the exit code without throwing.

## `GetExecOutputAsync` — capture stdout / stderr

```csharp
var output = await exec.GetExecOutputAsync("dotnet", ["--version"]);

Console.WriteLine($"exit code = {output.ExitCode}");
Console.WriteLine($"stdout    = {output.Stdout.Trim()}");
Console.WriteLine($"stderr    = {output.Stderr.Trim()}");
```

## `ExecOptions`

| Option | Description |
| --- | --- |
| `Cwd` | Working directory. Defaults to the current process's cwd. |
| `Env` | Replacement environment variable dictionary. Defaults to inheriting. |
| `Input` | Bytes to write to the child's `stdin`. The stream is closed after the write. |
| `Silent` | Suppress live mirror to `OutStream`/`ErrStream`. Defaults to `false`. |
| `FailOnStdErr` | Treat any `stderr` output as a failure. Defaults to `false`. |
| `IgnoreReturnCode` | Don't throw on non-zero exit codes. Defaults to `false`. |
| `OutStream` / `ErrStream` | Writers for the live mirror. Default to `Console.Out` / `Console.Error`. |
| `WindowsVerbatimArguments` | Windows-only: skip the runner's argument quoting. Defaults to `false`. |
| `Delay` | Milliseconds to wait for stdio drain after the child exits. Defaults to `10000`. |
| `Listeners` | Callbacks invoked for each chunk/line/debug message. |

## Listeners

```csharp
var stdoutChunks = 0;
string? lastErrLine = null;

var options = new ExecOptions
{
    Listeners = new ExecListeners
    {
        Stdout  = data    => stdoutChunks++,
        Stdline = line    => Console.WriteLine($"OUT> {line}"),
        Errline = line    => lastErrLine = line,
        Debug   = message => Console.Error.WriteLine($"[debug] {message}"),
    },
};

await exec.ExecAsync("dotnet", ["--info"], options);
```

## Cancellation

All methods honor `CancellationToken`. When triggered, the child process tree is killed:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

await exec.ExecAsync("some-tool", ["--blocking"], options: null, cts.Token);
```

## Attribution

This package is a .NET port of the official [`@actions/exec`](https://github.com/actions/toolkit/tree/main/packages/exec) Node.js package by GitHub, licensed under the [MIT License](https://github.com/actions/toolkit/blob/main/LICENSE.md). It mirrors the upstream public API surface (`exec`, `getExecOutput`, `ExecOptions`, `ExecOutput`, `ExecListeners`) and the `ToolRunner` quoting/argument-parsing behavior — including libuv's Windows `quote_cmd_arg` rules and the cmd.exe-specific quoting for `.cmd`/`.bat` invocations — where it makes sense in idiomatic .NET.

Tests in `tests/ActionsToolkitSharp.Exec.Tests` mirror the upstream `__tests__/exec.test.ts` file, using verbatim `it('…')` strings as `[Fact(DisplayName = "…")]` so upstream additions can be tracked over time.
