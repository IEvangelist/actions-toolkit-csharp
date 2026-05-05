# `ActionsToolkit.ToolCache` — file-based examples

These single-file scripts use .NET 10's
[`dotnet run app.cs`](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
support to demonstrate the `IToolCacheService` API exposed by
[`ActionsToolkit.ToolCache`](https://www.nuget.org/packages/ActionsToolkit.ToolCache).

Each script mirrors a section of the upstream
[`@actions/tool-cache` README](https://github.com/actions/toolkit/tree/main/packages/tool-cache).
The `#:package ActionsToolkit.ToolCache@*` directive at the top of each
script declares an inline NuGet reference, so no `.csproj` is required —
just `dotnet run <file>.cs`.

| File | Demonstrates | Run command |
| --- | --- | --- |
| [`download-and-cache.cs`](./download-and-cache.cs) | `DownloadToolAsync` + `ExtractTarAsync` + `CacheDirAsync` round-trip | `dotnet run download-and-cache.cs` |
| [`find.cs`](./find.cs) | `Find` (exact + range) and `FindAllVersions` against a populated cache | `dotnet run find.cs` |
| [`evaluate-versions.cs`](./evaluate-versions.cs) | `EvaluateVersions` for resolving a node-semver range to a concrete version | `dotnet run evaluate-versions.cs` |
| [`manifest.cs`](./manifest.cs) | `FindFromManifestAsync` against an in-memory manifest | `dotnet run manifest.cs` |

> ⚠️ The download-and-cache, find, and manifest scripts read the
> `RUNNER_TEMP` and `RUNNER_TOOL_CACHE` environment variables that the
> GitHub Actions runner sets. When running locally, point them at any
> writable directory, e.g.
> `RUNNER_TEMP=$(mktemp -d) RUNNER_TOOL_CACHE=$(mktemp -d) dotnet run find.cs`.

## Run them all

[`run-all.sh`](./run-all.sh) drives every example in sequence with
`set -euo pipefail`. It provisions per-script sandboxes via `mktemp -d` so
caches don't collide, and gates the network-bound `download-and-cache.cs`
behind `RUN_NETWORK_SAMPLES=true`.

```bash
chmod +x run-all.sh
./run-all.sh
```

## Use from a workflow

[`usage.yml`](./usage.yml) shows how to invoke a file-based ToolCache script
from a real workflow `run:` step.
