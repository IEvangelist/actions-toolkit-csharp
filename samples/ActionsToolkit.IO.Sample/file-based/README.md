# `ActionsToolkit.IO` — file-based examples

These single-file scripts use .NET 10's
[`dotnet run app.cs`](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
support to demonstrate the `IOperations` API exposed by
[`ActionsToolkit.IO`](https://www.nuget.org/packages/ActionsToolkit.IO).

Each script mirrors a section of the upstream
[`@actions/io` README](https://github.com/actions/toolkit/tree/main/packages/io).
The `#:package ActionsToolkit.IO@*` directive at the top of each
script declares an inline NuGet reference, so no `.csproj` is required —
just `dotnet run <file>.cs`.

| File | Demonstrates | Run command |
| --- | --- | --- |
| [`mkdir.cs`](./mkdir.cs) | `io.MakeDirectory(path)` — equivalent of `mkdir -p` | `dotnet run mkdir.cs` |
| [`cp-mv.cs`](./cp-mv.cs) | `io.Copy` with `CopyOptions(Recursive, Force)` and `io.Move` — equivalents of `cp -r` and `mv` | `dotnet run cp-mv.cs` |
| [`rm-rf.cs`](./rm-rf.cs) | `io.Remove(path)` for files and directories — equivalent of `rm -rf` | `dotnet run rm-rf.cs` |
| [`which.cs`](./which.cs) | `io.Which(tool)` and `io.FileInPath(tool)` — equivalent of `which` | `dotnet run which.cs` |

## Run them all

[`run-all.sh`](./run-all.sh) drives every example in sequence with
`set -euo pipefail`. The scripts use `Path.GetTempPath()` for any
filesystem mutations so they are idempotent and self-cleaning.

```bash
chmod +x run-all.sh
./run-all.sh
```

## Use from a workflow

[`usage.yml`](./usage.yml) shows how to invoke a file-based IO script
from a real workflow `run:` step.
