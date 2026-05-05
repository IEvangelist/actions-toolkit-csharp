# `ActionsToolkitSharp.Exec` — file-based examples

These single-file scripts use .NET 10's
[`dotnet run app.cs`](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
support to demonstrate the `IExecService` API exposed by
[`ActionsToolkitSharp.Exec`](https://www.nuget.org/packages/ActionsToolkitSharp.Exec).

Each script mirrors a section of the upstream
[`@actions/exec` README](https://github.com/actions/toolkit/tree/main/packages/exec).
The `#:package ActionsToolkitSharp.Exec@*` directive at the top of each
script declares an inline NuGet reference, so no `.csproj` is required —
just `dotnet run <file>.cs`.

| File | Demonstrates | Run command |
| --- | --- | --- |
| [`exec-basic.cs`](./exec-basic.cs) | `IExecService.ExecAsync(commandLine, args)` — basic streaming exec | `dotnet run exec-basic.cs` |
| [`get-output.cs`](./get-output.cs) | `IExecService.GetExecOutputAsync(...)` — capture `stdout`/`stderr` | `dotnet run get-output.cs` |
| [`with-listeners.cs`](./with-listeners.cs) | `ExecListeners.Stdline` and `ExecListeners.Stdout` — react to output as it streams | `dotnet run with-listeners.cs` |
| [`with-env.cs`](./with-env.cs) | `ExecOptions.Env` — pass a custom environment to the child process | `dotnet run with-env.cs` |
| [`with-cwd.cs`](./with-cwd.cs) | `ExecOptions.Cwd` — run a command from a different working directory | `dotnet run with-cwd.cs` |

## Run them all

[`run-all.sh`](./run-all.sh) drives every example in sequence with
`set -euo pipefail`. The scripts use `Path.GetTempPath()` for any
filesystem mutations so they are idempotent and self-cleaning.

```bash
chmod +x run-all.sh
./run-all.sh
```

## Use from a workflow

[`usage.yml`](./usage.yml) shows how to invoke a file-based Exec script
from a real workflow `run:` step.
