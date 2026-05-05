# `ActionsToolkit` samples

This directory hosts runnable samples for the
[`ActionsToolkit`](../README.md) family of NuGet packages — the
modern .NET port of the official
[`@actions/toolkit`](https://github.com/actions/toolkit) JavaScript
toolkit used by GitHub Actions authors.

Each package has two flavors of sample:

1. A traditional **project sample** (`Program.cs` + `.csproj`) that
   builds as part of the solution and runs in a Docker container as a
   container action. Existing examples live under
   `ActionsToolkit.Core.Sample` and `ActionsToolkit.Glob.Sample`.
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
| `ActionsToolkit.Core` | [`ActionsToolkit.Core.Sample/file-based/`](./ActionsToolkit.Core.Sample/file-based/) | [`inputs-and-outputs.cs`](./ActionsToolkit.Core.Sample/file-based/inputs-and-outputs.cs), [`export-variable.cs`](./ActionsToolkit.Core.Sample/file-based/export-variable.cs), [`set-secret.cs`](./ActionsToolkit.Core.Sample/file-based/set-secret.cs), [`add-path.cs`](./ActionsToolkit.Core.Sample/file-based/add-path.cs), [`exit-codes.cs`](./ActionsToolkit.Core.Sample/file-based/exit-codes.cs), [`logging.cs`](./ActionsToolkit.Core.Sample/file-based/logging.cs), [`groups.cs`](./ActionsToolkit.Core.Sample/file-based/groups.cs), [`annotations.cs`](./ActionsToolkit.Core.Sample/file-based/annotations.cs), [`styling-output.cs`](./ActionsToolkit.Core.Sample/file-based/styling-output.cs), [`action-state.cs`](./ActionsToolkit.Core.Sample/file-based/action-state.cs), [`job-summary.cs`](./ActionsToolkit.Core.Sample/file-based/job-summary.cs) |
| `ActionsToolkit.Glob` | [`ActionsToolkit.Glob.Sample/file-based/`](./ActionsToolkit.Glob.Sample/file-based/) | [`basic.cs`](./ActionsToolkit.Glob.Sample/file-based/basic.cs), [`recursive.cs`](./ActionsToolkit.Glob.Sample/file-based/recursive.cs), [`iterator.cs`](./ActionsToolkit.Glob.Sample/file-based/iterator.cs), [`glob-with-input.cs`](./ActionsToolkit.Glob.Sample/file-based/glob-with-input.cs), [`builder.cs`](./ActionsToolkit.Glob.Sample/file-based/builder.cs) |
| `ActionsToolkit.IO` | [`ActionsToolkit.IO.Sample/file-based/`](./ActionsToolkit.IO.Sample/file-based/) | [`mkdir.cs`](./ActionsToolkit.IO.Sample/file-based/mkdir.cs), [`cp-mv.cs`](./ActionsToolkit.IO.Sample/file-based/cp-mv.cs), [`rm-rf.cs`](./ActionsToolkit.IO.Sample/file-based/rm-rf.cs), [`which.cs`](./ActionsToolkit.IO.Sample/file-based/which.cs) |
| `ActionsToolkit.Exec` | [`ActionsToolkit.Exec.Sample/file-based/`](./ActionsToolkit.Exec.Sample/file-based/) | [`exec-basic.cs`](./ActionsToolkit.Exec.Sample/file-based/exec-basic.cs), [`get-output.cs`](./ActionsToolkit.Exec.Sample/file-based/get-output.cs), [`with-listeners.cs`](./ActionsToolkit.Exec.Sample/file-based/with-listeners.cs), [`with-env.cs`](./ActionsToolkit.Exec.Sample/file-based/with-env.cs), [`with-cwd.cs`](./ActionsToolkit.Exec.Sample/file-based/with-cwd.cs) |
| `ActionsToolkit.Octokit` | [`ActionsToolkit.Octokit.Sample/file-based/`](./ActionsToolkit.Octokit.Sample/file-based/) | [`get-octokit.cs`](./ActionsToolkit.Octokit.Sample/file-based/get-octokit.cs), [`context.cs`](./ActionsToolkit.Octokit.Sample/file-based/context.cs), [`create-issue.cs`](./ActionsToolkit.Octokit.Sample/file-based/create-issue.cs) |
| `ActionsToolkit.HttpClient` | [`ActionsToolkit.HttpClient.Sample/file-based/`](./ActionsToolkit.HttpClient.Sample/file-based/) | [`get-todos.cs`](./ActionsToolkit.HttpClient.Sample/file-based/get-todos.cs), [`bearer-token.cs`](./ActionsToolkit.HttpClient.Sample/file-based/bearer-token.cs) |
| `ActionsToolkit.Attest` | [`ActionsToolkit.Attest.Sample/file-based/`](./ActionsToolkit.Attest.Sample/file-based/) | [`attest-provenance.cs`](./ActionsToolkit.Attest.Sample/file-based/attest-provenance.cs), [`attest-custom.cs`](./ActionsToolkit.Attest.Sample/file-based/attest-custom.cs), [`attest-with-headers.cs`](./ActionsToolkit.Attest.Sample/file-based/attest-with-headers.cs) |
| `ActionsToolkit.Cache` | [`ActionsToolkit.Cache.Sample/file-based/`](./ActionsToolkit.Cache.Sample/file-based/) | [`restore-and-save.cs`](./ActionsToolkit.Cache.Sample/file-based/restore-and-save.cs), [`lookup-only.cs`](./ActionsToolkit.Cache.Sample/file-based/lookup-only.cs), [`cross-os-archive.cs`](./ActionsToolkit.Cache.Sample/file-based/cross-os-archive.cs), [`with-options.cs`](./ActionsToolkit.Cache.Sample/file-based/with-options.cs) |

> **Phase 2 package — `ActionsToolkit.ToolCache` —
> does not exist yet.** Its file-based samples will be added when that
> package lands.

## Running a sample

Every file-based example can be run directly with `dotnet run` —
no scaffold project needed:

```bash
cd samples/ActionsToolkit.Core.Sample/file-based
dotnet run logging.cs
```

The `#:package ActionsToolkit.<Pkg>@*` directives at the top of
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
* Each package's own README in `src/ActionsToolkit.<Pkg>/README.md`
