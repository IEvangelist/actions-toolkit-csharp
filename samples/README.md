# `ActionsToolkitSharp` samples

This directory hosts runnable samples for the
[`ActionsToolkitSharp`](../README.md) family of NuGet packages — the
modern .NET port of the official
[`@actions/toolkit`](https://github.com/actions/toolkit) JavaScript
toolkit used by GitHub Actions authors.

Each package has two flavors of sample:

1. A traditional **project sample** (`Program.cs` + `.csproj`) that
   builds as part of the solution and runs in a Docker container as a
   container action. Existing examples live under
   `ActionsToolkitSharp.Core.Sample` and `ActionsToolkitSharp.Glob.Sample`.
2. A new **file-based sample** under each package's `file-based/` sub-folder.
   These are single-file C# scripts that take advantage of .NET 10's
   [`dotnet run app.cs`](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
   feature and `#:package` directive for inline NuGet references — no
   `.csproj` required.

The file-based samples are designed to mirror the upstream
`@actions/<package>` README code blocks so a contributor familiar with
the official Node toolkit can read both side-by-side and see how the
modern C# API maps over.

## Index of file-based samples

| Package | Folder | Examples |
| --- | --- | --- |
| `ActionsToolkitSharp.Core` | [`ActionsToolkitSharp.Core.Sample/file-based/`](./ActionsToolkitSharp.Core.Sample/file-based/) | [`inputs-and-outputs.cs`](./ActionsToolkitSharp.Core.Sample/file-based/inputs-and-outputs.cs), [`export-variable.cs`](./ActionsToolkitSharp.Core.Sample/file-based/export-variable.cs), [`set-secret.cs`](./ActionsToolkitSharp.Core.Sample/file-based/set-secret.cs), [`add-path.cs`](./ActionsToolkitSharp.Core.Sample/file-based/add-path.cs), [`exit-codes.cs`](./ActionsToolkitSharp.Core.Sample/file-based/exit-codes.cs), [`logging.cs`](./ActionsToolkitSharp.Core.Sample/file-based/logging.cs), [`groups.cs`](./ActionsToolkitSharp.Core.Sample/file-based/groups.cs), [`annotations.cs`](./ActionsToolkitSharp.Core.Sample/file-based/annotations.cs), [`styling-output.cs`](./ActionsToolkitSharp.Core.Sample/file-based/styling-output.cs), [`action-state.cs`](./ActionsToolkitSharp.Core.Sample/file-based/action-state.cs), [`job-summary.cs`](./ActionsToolkitSharp.Core.Sample/file-based/job-summary.cs) |
| `ActionsToolkitSharp.Glob` | [`ActionsToolkitSharp.Glob.Sample/file-based/`](./ActionsToolkitSharp.Glob.Sample/file-based/) | [`basic.cs`](./ActionsToolkitSharp.Glob.Sample/file-based/basic.cs), [`recursive.cs`](./ActionsToolkitSharp.Glob.Sample/file-based/recursive.cs), [`iterator.cs`](./ActionsToolkitSharp.Glob.Sample/file-based/iterator.cs), [`glob-with-input.cs`](./ActionsToolkitSharp.Glob.Sample/file-based/glob-with-input.cs), [`builder.cs`](./ActionsToolkitSharp.Glob.Sample/file-based/builder.cs) |
| `ActionsToolkitSharp.IO` | [`ActionsToolkitSharp.IO.Sample/file-based/`](./ActionsToolkitSharp.IO.Sample/file-based/) | [`mkdir.cs`](./ActionsToolkitSharp.IO.Sample/file-based/mkdir.cs), [`cp-mv.cs`](./ActionsToolkitSharp.IO.Sample/file-based/cp-mv.cs), [`rm-rf.cs`](./ActionsToolkitSharp.IO.Sample/file-based/rm-rf.cs), [`which.cs`](./ActionsToolkitSharp.IO.Sample/file-based/which.cs) |
| `ActionsToolkitSharp.Octokit` | [`ActionsToolkitSharp.Octokit.Sample/file-based/`](./ActionsToolkitSharp.Octokit.Sample/file-based/) | [`get-octokit.cs`](./ActionsToolkitSharp.Octokit.Sample/file-based/get-octokit.cs), [`context.cs`](./ActionsToolkitSharp.Octokit.Sample/file-based/context.cs), [`create-issue.cs`](./ActionsToolkitSharp.Octokit.Sample/file-based/create-issue.cs) |
| `ActionsToolkitSharp.HttpClient` | [`ActionsToolkitSharp.HttpClient.Sample/file-based/`](./ActionsToolkitSharp.HttpClient.Sample/file-based/) | [`get-todos.cs`](./ActionsToolkitSharp.HttpClient.Sample/file-based/get-todos.cs), [`bearer-token.cs`](./ActionsToolkitSharp.HttpClient.Sample/file-based/bearer-token.cs) |

> **Phase 2 packages — `ActionsToolkitSharp.Exec` and `ActionsToolkitSharp.ToolCache`
> — do not exist yet.** Their file-based samples will be added when those
> packages land.

## Running a sample

Every file-based example can be run directly with `dotnet run` —
no scaffold project needed:

```bash
cd samples/ActionsToolkitSharp.Core.Sample/file-based
dotnet run logging.cs
```

The `#:package ActionsToolkitSharp.<Pkg>@*` directives at the top of
each script declare inline NuGet references. `@*` resolves to the
latest available version on whichever feeds your `nuget.config` lists.
In CI we rewrite `@*` to the locally packed version published to a
private feed so the samples can be exercised end-to-end before
publishing.

Each sub-folder also ships:

* `run-all.sh` — drives every example with the env vars a real Action
  step would set (`INPUT_*`, `GITHUB_OUTPUT`, `GITHUB_ENV`,
  `GITHUB_PATH`, `GITHUB_STATE`, `GITHUB_STEP_SUMMARY`, …) and prints
  the resulting file-command files. Idempotent and protected with
  `set -euo pipefail`.
* `usage.yml` — a minimal workflow snippet showing how to invoke the
  same script from a `run: shell: bash` step in a real GitHub Actions
  workflow.

## See also

* Upstream JavaScript toolkit: <https://github.com/actions/toolkit>
* `dotnet run app.cs` announcement: <https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/>
* Each package's own README in `src/ActionsToolkitSharp.<Pkg>/README.md`
