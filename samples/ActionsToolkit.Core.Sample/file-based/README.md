# `ActionsToolkit.Core` — file-based examples

These are single-file C# scripts you can run with .NET 10's
[`dotnet run app.cs`](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
support. Each one mirrors a section of the upstream
[`@actions/core` README](https://github.com/actions/toolkit/tree/main/packages/core)
but uses the modern, dependency-injection-friendly C# API exposed by
`ActionsToolkit.Core`.

The `#:package ActionsToolkit.Core@*` directive at the top of each
script declares an inline NuGet reference, so no `.csproj` is required —
just `dotnet run <file>.cs`. CI rewrites `@*` to the locally packed
version published to a private feed; in published docs you can pin to
a real version such as `@9.0.0`.

| File | Demonstrates | Run command |
| --- | --- | --- |
| [`inputs-and-outputs.cs`](./inputs-and-outputs.cs) | `GetInput`, `GetBoolInput`, `GetMultilineInput`, `SetOutputAsync` | `INPUT_INPUTNAME=hello INPUT_BOOLINPUTNAME=true INPUT_MULTILINEINPUTNAME=$'one\ntwo' GITHUB_OUTPUT=$(mktemp) dotnet run inputs-and-outputs.cs` |
| [`export-variable.cs`](./export-variable.cs) | `ExportVariableAsync` (writes to `$GITHUB_ENV`) | `GITHUB_ENV=$(mktemp) dotnet run export-variable.cs` |
| [`set-secret.cs`](./set-secret.cs) | `SetSecret` (emits the `add-mask` workflow command) | `dotnet run set-secret.cs` |
| [`add-path.cs`](./add-path.cs) | `AddPathAsync` (writes to `$GITHUB_PATH`) | `GITHUB_PATH=$(mktemp) dotnet run add-path.cs` |
| [`exit-codes.cs`](./exit-codes.cs) | `SetFailed` and the standard try/catch pattern | `dotnet run exit-codes.cs` |
| [`logging.cs`](./logging.cs) | `WriteDebug`, `WriteInfo`, `WriteNotice`, `WriteWarning`, `WriteError`, `IsDebug` | `INPUT_INPUT=hello dotnet run logging.cs` |
| [`groups.cs`](./groups.cs) | `StartGroup`, `EndGroup`, `GroupAsync` | `dotnet run groups.cs` |
| [`annotations.cs`](./annotations.cs) | Annotation messages with `AnnotationProperties` | `dotnet run annotations.cs` |
| [`styling-output.cs`](./styling-output.cs) | ANSI escape codes for foreground/background colors and styles | `dotnet run styling-output.cs` |
| [`action-state.cs`](./action-state.cs) | `SaveStateAsync` and `GetState` (wrapper-action `post:` step pattern) | `GITHUB_STATE=$(mktemp) STATE_PIDTOKILL=12345 dotnet run action-state.cs` |
| [`job-summary.cs`](./job-summary.cs) | `Summary.Add*` builders and `Summary.WriteAsync` | `GITHUB_STEP_SUMMARY=$(mktemp) dotnet run job-summary.cs` |

## Run them all

[`run-all.sh`](./run-all.sh) drives each script end-to-end with the
environment variables a real GitHub Actions runner would set
(`INPUT_*`, `GITHUB_OUTPUT`, `GITHUB_ENV`, `GITHUB_PATH`, `GITHUB_STATE`,
`GITHUB_STEP_SUMMARY`), then `cat`s each file-command file so you can
see exactly what the action wrote.

```bash
chmod +x run-all.sh
./run-all.sh
```

## Use from a workflow

[`usage.yml`](./usage.yml) shows how to invoke any of these scripts from
a `run:` step inside a real workflow. The pattern is the same regardless
of which file you choose.
