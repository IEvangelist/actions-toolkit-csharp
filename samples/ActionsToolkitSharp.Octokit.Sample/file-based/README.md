# `ActionsToolkitSharp.Octokit` — file-based examples

These single-file scripts use .NET 10's
[`dotnet run app.cs`](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
support to demonstrate the API exposed by
[`ActionsToolkitSharp.Octokit`](https://www.nuget.org/packages/ActionsToolkitSharp.Octokit).

Each script mirrors a section of the upstream
[`@actions/github` README](https://github.com/actions/toolkit/tree/main/packages/github).
Under the hood the package wraps
[`GitHub.Octokit.SDK`](https://www.nuget.org/packages/GitHub.Octokit.SDK)
(the Kiota-generated GitHub REST client) and registers a hydrated
`GitHubClient` plus a `Context` materialized from the workflow's
environment variables.

The `#:package ActionsToolkitSharp.Octokit@*` directive at the top of
each script declares an inline NuGet reference, so no `.csproj` is
required — just `dotnet run <file>.cs`.

| File | Demonstrates | Run command |
| --- | --- | --- |
| [`get-octokit.cs`](./get-octokit.cs) | Reading `myToken` via `ICoreService.GetInput`, registering `GitHubClient` via `AddGitHubClientServices(token)`, calling `client.Repos["octokit"]["rest.js"].Pulls[123].GetAsync()` | `INPUT_MYTOKEN=$GITHUB_TOKEN dotnet run get-octokit.cs` |
| [`context.cs`](./context.cs) | `Context.Current` — surfacing repository, ref, sha, event, run id, etc. from the runner's environment | `GITHUB_REPOSITORY=octocat/Hello-World GITHUB_REF=refs/heads/main ... dotnet run context.cs` |
| [`create-issue.cs`](./create-issue.cs) | Combining `Context.Current.Repo` with the Kiota client to create an issue (the upstream `octokit.rest.issues.create({...context.repo, title, body})` pattern) | `INPUT_MYTOKEN=$GITHUB_TOKEN GITHUB_REPOSITORY=owner/repo dotnet run create-issue.cs` |

## Run them all

[`run-all.sh`](./run-all.sh) drives every example in sequence with
`set -euo pipefail`. Examples that need real GitHub credentials are
skipped automatically when `GITHUB_TOKEN` is not set, so the driver is
safe to run locally.

```bash
chmod +x run-all.sh
./run-all.sh
```

## Use from a workflow

[`usage.yml`](./usage.yml) shows how to wire `secrets.GITHUB_TOKEN`
through to a file-based Octokit script in a real workflow `run:` step.
