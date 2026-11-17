# Run C#

A public **Linux Docker action**, with a native .NET launcher and the official
.NET **11.0.100 SDK**. No Node runtime or shell entrypoint. Checkout is required.
The image is built from this directory on the runner; there is no registry
publication step. The SDK remains in the final image to compile file-based apps.

## Usage

After checkout, reference this action at an **audited full commit SHA**:

```yaml
- name: Run a checked-in script
  id: report
  uses: IEvangelist/actions-toolkit-csharp/actions/run-csharp@<full-commit-sha>
  with:
    script: .github/scripts/report.cs
    args: '["two words", "--format", "json"]'
    inputs: '{"report name":"nightly","enabled":"true"}'
    # Optional: explicitly opt into GitHub API access with least privilege.
    # github-token: ${{ github.token }}
- name: Read the script output
  env:
    REPORT: ${{ steps.report.outputs.report }}
  run: printf '%s\n' "$REPORT"
```

Replace the SHA placeholder; it is intentionally not a moving release ref.
Use `runs-on: ubuntu-latest` (or a supported Linux runner with Docker).
The downstream `run` above is an ordinary workflow step, not the action's
entrypoint. Scripts can write normal runner outputs, for example
`Core.SetOutputAsync("report", "ready")` when using ActionsToolkit.Sdk.
Use `toJSON(...)` when mapping expression values into the JSON inputs rather
than interpolating unescaped data into JSON.

| Input | Meaning |
| --- | --- |
| `script` | Required existing workspace-relative `.cs` file; use `/` separators. Absolute paths, traversal outside the workspace and escaping symlinks are rejected. |
| `args` | JSON array of strings, default `[]`. Empty strings, spaces, quotes and punctuation remain literal arguments; no shell expansion. |
| `inputs` | JSON object of string values, default `{}`. `report name` becomes `INPUT_REPORT_NAME`; hyphens are preserved, matching toolkit normalization. |
| `github-token` | Optional explicit token, masked before launch and passed as `GITHUB_TOKEN` in the child environment. **No automatic token default.** |

Input names must start with an ASCII letter/digit and contain only ASCII
letters/digits, spaces, underscores or hyphens. Duplicate keys (including
case/space normalization collisions), NUL values, launcher control names
(`script`, `args`, `inputs`, `github-token`, `github_token`) and normalized
`GITHUB_*`, `ACTIONS_*`, `RUNNER_*` names are rejected. Values must be strings,
not JSON numbers, booleans, objects or null.

## Execution and credentials

The launcher resolves the real workspace and script paths, including symbolic
links, then runs `dotnet run --file <resolved-script> -- <arguments>` with
`ProcessStartInfo.ArgumentList` and `UseShellExecute=false`. The working directory
is the real workspace, and standard input/output/error are inherited.
Validation occurs before starting the child. Treat scripts and their NuGet/MSBuild
dependencies as executable code: path validation is not a security sandbox, and
concurrent mutation of checked-out paths must not be used to bypass it.

Runner-provided environment is inherited, including cache/results/OIDC credentials
and translated `GITHUB_OUTPUT`, `GITHUB_ENV`, `GITHUB_PATH`, `GITHUB_STATE` and
`GITHUB_STEP_SUMMARY` paths. Dedicated `ACTIONS_TOOLKIT_*` launcher controls and the
action's `INPUT_SCRIPT`, `INPUT_ARGS`, `INPUT_INPUTS`, `INPUT_GITHUB-TOKEN` and
`INPUT_GITHUB_TOKEN` variables are removed before applying script inputs.
Other inherited inputs remain available. If no token is supplied, any
already-inherited `GITHUB_TOKEN` is left unchanged; no token is fabricated.

The explicit token is escaped in an `add-mask` stdout command and never placed
on the command line, written to environment/output files, or dumped in diagnostics.
Scripts can still disclose credentials themselves; only run trusted code.
OIDC requires `id-token: write`; API access requires explicit credentials and
appropriate permissions. Docker does not grant extra permissions or bypass
GitHub Enterprise/backend limitations. Runtime credentials are supplied by
GitHub's **Docker action handler**: a job-level container or manual `docker run`
does not prove those capabilities work.

Child nonzero exit codes are returned unchanged; validation/start failures return
1 with a credential-safe error. Linux SIGTERM/SIGINT cancel the wait, forcibly
terminate the child process tree and wait for cleanup (up to 10 seconds),
then return 143/130. Shutdown is intentionally forceful, not a graceful
application signal handler. There is no shell between PID 1 and the launcher.

## Container boundary

The official `mcr.microsoft.com/dotnet/sdk:11.0.100` manifest-list digest is pinned
in the Dockerfile (verified against the registry). Updating the SDK requires
an intentional digest update. The action builds only its framework-only launcher;
there are no repository-parent build configuration or package dependencies.

Image pulling/building and script restore/compilation add cold-start overhead.
The action honors compatible workspace `global.json` files; incompatible SDK
requests fail normally rather than downloading another SDK or rewriting files.
The CLI selects the .NET SDK from the workspace working directory. File-based
apps can separately use a script-adjacent `global.json` **`msbuild-sdks` mapping**
and `NuGet.Config` with a relative local feed to resolve a custom MSBuild SDK.
A script-adjacent `sdk.version` alone does not change the CLI's .NET SDK selection.
Host `setup-dotnet`, SDK caches and host-installed tools do not change this image.
Only container-installed tools are available. For other toolchains use the direct
host execution path or a reviewed derivative image; do not mount the Docker socket
or additional host directories.

The image keeps the official default root user for GitHub Docker action mounted
workspace/file-command access. Generated workspace files may therefore be
root-owned on Linux; do not assume host-user ownership. No ownership rewriting is
performed. Separate action invocations share only normal runner files/outputs,
not .NET objects, processes or buffered summaries. `GITHUB_PATH` additions naming
container-only tools do not make those tools available on the host.

## Local checks

From the repository root:

```text
dotnet test tests/ActionsToolkit.Runner.Tests/ActionsToolkit.Runner.Tests.csproj
docker build --tag actions-toolkit-run-csharp:local actions/run-csharp
```

The Docker build context is **this action directory**, not the repository root.
The launcher test project covers real bounded .NET child processes, input/path
validation, environment isolation, file-command writes and cancellation.
Local container checks are not a substitute for real GitHub runner integration:
validate cache/artifact/OIDC credentials, step-output handoff, mounted path
translation, ownership and PID 1 signal handling on the target Linux runner
before release. No automatic post-job cache save, SDK/tool installation, package
publication or registry publication is performed.
