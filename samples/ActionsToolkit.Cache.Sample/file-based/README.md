# `ActionsToolkit.Cache` file-based samples

These single-file C# scripts mirror the upstream
[`@actions/cache`](https://github.com/actions/toolkit/tree/main/packages/cache)
README code blocks. Each file uses .NET 10's `dotnet run app.cs` feature plus
the `#:package` directive for inline NuGet references.

| Example | Mirrors upstream section |
| --- | --- |
| [`restore-and-save.cs`](./restore-and-save.cs) | [`restoreCache` + `saveCache`](https://github.com/actions/toolkit/tree/main/packages/cache#restore-cache) |
| [`lookup-only.cs`](./lookup-only.cs) | [`lookupOnly` option](https://github.com/actions/toolkit/tree/main/packages/cache#options) |
| [`cross-os-archive.cs`](./cross-os-archive.cs) | [`enableCrossOsArchive` flag](https://github.com/actions/toolkit/tree/main/packages/cache#options) |
| [`with-options.cs`](./with-options.cs) | [`UploadOptions` / `DownloadOptions`](https://github.com/actions/toolkit/tree/main/packages/cache#options) |

## Required environment

These scripts rely on the runtime variables that GitHub Actions injects into
every job:

| Variable | Purpose |
| --- | --- |
| `ACTIONS_CACHE_SERVICE_V2` | Set to `1` to opt in to the V2 Twirp+signed-URL backend (the default for hosted runners). |
| `ACTIONS_RESULTS_URL` | Base URL of the V2 cache service (`https://results-receiver.actions.githubusercontent.com/`). |
| `ACTIONS_RUNTIME_TOKEN` | OAuth/Actions token used as bearer credential on every Twirp call. |
| `GITHUB_WORKSPACE` | Root directory the cache paths are resolved against (defaults to the current working directory if unset). |
| `RUNNER_TEMP` | Optional staging directory for the tar+zstd archive (falls back to the OS temp directory). |

## Run

```bash
cd samples/ActionsToolkit.Cache.Sample/file-based
dotnet run restore-and-save.cs build-linux-abc123
```

The companion `usage.yml` shows the GitHub Actions workflow that wires every
script into a real CI job.
