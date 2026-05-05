# `ActionsToolkitSharp.Attest` file-based samples

These single-file C# scripts mirror the upstream
[`@actions/attest`](https://github.com/actions/toolkit/tree/main/packages/attest)
README code blocks. Each file uses .NET 10's `dotnet run app.cs` feature plus
the `#:package` directive for inline NuGet references.

| Example | Mirrors upstream section |
| --- | --- |
| [`attest-provenance.cs`](./attest-provenance.cs) | [`attestProvenance`](https://github.com/actions/toolkit/tree/main/packages/attest#attestprovenance) |
| [`attest-custom.cs`](./attest-custom.cs) | [`attest`](https://github.com/actions/toolkit/tree/main/packages/attest#attest) |
| [`attest-with-headers.cs`](./attest-with-headers.cs) | [Options](https://github.com/actions/toolkit/tree/main/packages/attest#options) |

## Required environment

These scripts rely on the runtime variables that GitHub Actions injects into
every job:

| Variable | Purpose |
| --- | --- |
| `GITHUB_TOKEN` | OAuth/Actions token used to write attestations. |
| `GITHUB_REPOSITORY` | `owner/repo` slug used to address the API. |
| `GITHUB_SERVER_URL` | Server URL, used by `attestProvenance` for the SLSA `repository` external parameter. |
| `ACTIONS_ID_TOKEN_REQUEST_URL` / `ACTIONS_ID_TOKEN_REQUEST_TOKEN` | OIDC envelope used to mint signing tokens against Sigstore. |

## Run

```bash
cd samples/ActionsToolkitSharp.Attest.Sample/file-based
dotnet run attest-provenance.cs ./my-artifact.tar.gz
```

The companion `usage.yml` shows the GitHub Actions workflow that wires every
script into a real CI job.
