# C# Actions showcase

Thirteen independent workflows demonstrate real C# automation and step-to-step
results. Each has a manual **Run workflow** trigger. None of the workflows has
been run merely by checking out this source; inspect actual run results in Actions.

| Workflow | What the next C# step proves |
| --- | --- |
| [01 Inline C#](../../.github/workflows/showcase-01-inline-csharp.yml) | Stdin-produced output arrives as an input |
| [02 Typed inputs](../../.github/workflows/showcase-02-typed-inputs.yml) | String, Boolean, and multiline values validate correctly |
| [03 Step outputs](../../.github/workflows/showcase-03-step-outputs.yml) | Multiline text and source-generated JSON survive the handoff |
| [04 Environment/PATH](../../.github/workflows/showcase-04-environment-path.yml) | A later host step resolves a published C# tool |
| [05 Diagnostics](../../.github/workflows/showcase-05-diagnostics.yml) | Notices/warnings accompany explicit validation results |
| [06 Summaries](../../.github/workflows/showcase-06-job-summary.yml) | A summary uses checked, measured repository data |
| [07 Processes](../../.github/workflows/showcase-07-process-execution.yml) | The built assembly works; exit codes remain meaningful |
| [08 Filesystem](../../.github/workflows/showcase-08-glob-filesystem.yml) | Every glob-selected staged file matches its SHA-256 manifest |
| [09 GitHub/HTTP](../../.github/workflows/showcase-09-github-http.yml) | Two typed clients agree on repository identity |
| [10 Tool cache](../../.github/workflows/showcase-10-tool-cache.yml) | A pinned, digest-verified jq binary can be found and executed |
| [11 Cache/artifacts](../../.github/workflows/showcase-11-cache-artifacts.yml) | Separate Docker invocations restore identical cache/artifact content |
| [12 AOT/provenance](../../.github/workflows/showcase-12-aot-attestation.yml) | The native app runs and GitHub CLI independently verifies provenance |
| [13 File statistics](../../.github/workflows/showcase-13-glob-file-stats.yml) | Glob counts/bytes agree with a report before rich summary rendering |

Find them by their `C# showcase` names in the repository's **Actions** tab.
New manual workflows must be present on the default branch before GitHub exposes
their dispatch trigger. Before then, dispatch `build-and-test` on the feature
branch with `verify_sdk` enabled to call the SDK consumer and showcase workflows
at that branch's commit. The provenance workflow is restricted to the default branch
and needs `id-token: write` and `attestations: write`. GitHub/HTTP uses the job's
read-only token. Other host examples need only `contents: read`. Tool downloads
need outbound network access; all SDK restores need configured NuGet feeds.

## Try this checkout

```powershell
./eng/Stage-SdkShowcase.ps1
./eng/Test-SdkConsumers.ps1
```

Staging builds local packages, copies samples to `.showcase`, supplies an isolated
build configuration, and pins the exact SDK in `.showcase/global.json`. Source
examples therefore use `#:sdk ActionsToolkit.Sdk` without floating versions.
This bootstrap is for repository development, not a requirement for users of a
published release. Build outputs under `artifacts` and `.showcase` are ignored.

For a published release, use `#:sdk ActionsToolkit.Sdk@<actual-version>` directly.
Alternatively, pin `msbuild-sdks` in your own `global.json`. Never put the literal
`<version>` placeholder or an unavailable release into a production workflow.
The existing tag-triggered publish workflow packs/publishes both new packages
with the library release; publication and tagging are separate release actions.

## Inline C# and standard step wiring

After checkout and `actions/setup-dotnet`, a host step can use a quoted Bash
heredoc. Stdin scripts use an exact package version in `#:sdk`; this repository's
setup action exports the staged version as `ACTIONS_TOOLKIT_SDK_VERSION` and
preloads its package into the local NuGet cache:

```yaml
- id: producer
  shell: bash
  run: |
    dotnet run - <<'CSHARP'
    #:sdk ActionsToolkit.Sdk@${{ env.ACTIONS_TOOLKIT_SDK_VERSION }}
    await Core.SetOutputAsync("message", "Hello from C#!");
    CSHARP
- shell: bash
  env:
    INPUT_MESSAGE: ${{ steps.producer.outputs.message }}
  run: |
    dotnet run - <<'CSHARP'
    #:sdk ActionsToolkit.Sdk@${{ env.ACTIONS_TOOLKIT_SDK_VERSION }}
    Core.Info(Core.GetInput("message", required: true));
    CSHARP
```

The PowerShell equivalent is a single-quoted here-string:

```yaml
- shell: pwsh
  run: |
    @'
    #:sdk ActionsToolkit.Sdk@${{ env.ACTIONS_TOOLKIT_SDK_VERSION }}
    Core.Info("Hello from PowerShell stdin");
    '@ | dotnet run -
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
```

Use `dotnet run --file script.cs -- arguments` for checked-in scripts, and native
`#:include helpers.cs` for shared declarations. Stdin is consumed as source and
cannot simultaneously supply interactive input. Its working directory is the
build/run context; do not assume there is an original `.cs` file directory.
Repository MSBuild/NuGet/global.json configuration still applies.

Never inject untrusted GitHub expressions into C# source. Quoted heredocs prevent
shell expansion, not GitHub expression evaluation. Pass data through `env`,
inputs, or arguments. Outputs are strings; use explicit source-generated JSON
metadata for structures and files/artifacts for larger payloads. Normal GitHub
output size limits and secret filtering apply.

## Host or Docker?

| | Host with setup-dotnet | Native Docker companion |
| --- | --- | --- |
| Platforms | Windows, Linux, macOS | Linux with Docker |
| SDK/tools | User-installed tools and global.json | Bundled SDK and image tools |
| Launch | `dotnet run --file` or `dotnet run -` | `uses: .../actions/run-csharp@<commit>` |
| Runtime cache/artifact credentials | Not assumed in plain shell steps | Provided by GitHub's Docker action handler |

The companion has no Node.js or shell entrypoint. Its C# launcher starts the same
file-based app. It does not import host SDK installations, grant permissions,
export tokens to subsequent steps, or add container-only tool paths to the host.
Image builds/restores cost time; it is an explicit capability path, not a silent
fallback. Pin external actions by immutable commit and images by digest.

All ten libraries restore with the SDK even if a script only uses Core. Lazy
clients reduce initialization, not restore cost. Network clients use normal
`using` ownership. Summary buffers belong to one process and require an explicit
`WriteAsync`; the next C# step does not inherit in-memory state.

For Markdown summaries, use `AddMarkdownHeading`, `AddRawMarkdown`, and
`AddMarkdownCodeBlock`, with blank lines between blocks. `AddHeading`,
`AddTable`, and `AddCodeBlock` emit HTML instead; Markdown following an HTML
block needs a blank-line boundary. `AddRawMarkdown` handles that mode change,
whereas `AddRaw` appends text without selecting a format.

Normal scripts need not native-publish first. File-based AOT publishing defaults
remain intact; native builds require the platform C++/linker prerequisites.
`aot-app.cs` demonstrates native includes, optional static imports, and C# 15
collection-expression arguments. `#:property PublishAot=false` explicitly opts
out for managed publishing. Third-party dependencies retain their own AOT limits.

The source is inspired by the module ergonomics of
[actions/toolkit](https://github.com/actions/toolkit) and the custom-SDK authoring
experience of [Cake.Sdk](https://github.com/cake-build/generator).
