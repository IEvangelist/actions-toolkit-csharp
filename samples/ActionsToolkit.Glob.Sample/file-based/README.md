# `ActionsToolkit.Glob` — file-based examples

These single-file scripts use .NET 10's
[`dotnet run app.cs`](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
support to demonstrate the API exposed by
[`ActionsToolkit.Glob`](https://www.nuget.org/packages/ActionsToolkit.Glob).

Each script mirrors a section of the upstream
[`@actions/glob` README](https://github.com/actions/toolkit/tree/main/packages/glob)
but uses the modern, allocation-friendly C# `Globber` / `IGlobPatternResolverBuilder`
APIs. The `#:package ActionsToolkit.Glob@*` directive at the top of
each script declares an inline NuGet reference, so no `.csproj` is
required — just `dotnet run <file>.cs`.

| File | Demonstrates | Run command |
| --- | --- | --- |
| [`basic.cs`](./basic.cs) | `Globber.Create(patterns)` with an inclusion list | `dotnet run basic.cs` |
| [`recursive.cs`](./recursive.cs) | `Globber.Create("**/*")` to walk every file | `dotnet run recursive.cs` |
| [`iterator.cs`](./iterator.cs) | Streaming results via `foreach (var file in globber.GlobFiles())` | `dotnet run iterator.cs` |
| [`glob-with-input.cs`](./glob-with-input.cs) | Combining `ICoreService.GetInput("files")` with `Globber.Create` (mirrors the README's "Recommended action inputs" pattern) | `INPUT_FILES="**/*.cs" dotnet run glob-with-input.cs` |
| [`builder.cs`](./builder.cs) | `IGlobPatternResolverBuilder.WithInclusions / WithExclusions / Build` for include + exclude patterns | `dotnet run builder.cs` |

## Run them all

[`run-all.sh`](./run-all.sh) drives every example in sequence with
`set -euo pipefail`. It is idempotent and creates a temporary scratch
directory so any side effects are isolated.

```bash
chmod +x run-all.sh
./run-all.sh
```

## Use from a workflow

[`usage.yml`](./usage.yml) shows how to invoke `glob-with-input.cs` from
a real workflow `run:` step.
