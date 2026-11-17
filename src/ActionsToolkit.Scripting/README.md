# ActionsToolkit.Scripting

Typed, AOT-compatible helpers for C# in GitHub Actions. This package references all ten toolkit libraries and also works as an ordinary `PackageReference`; no host, source generator, or bootstrap is required.

```csharp
using ActionsToolkit.Scripting;

var name = Core.GetInput("name", required: true);
Core.Info($"Hello, {name}");
await Core.SetOutputAsync("ready", true);
await Exec.RunAsync("dotnet", ["--info"]);

using var http = Http.CreateBearerTokenClient(Core.GetInput("token", required: true));
using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/zen");
using var response = await http.SendAsync(request);
response.EnsureSuccessStatusCode();
```

## Modules and ownership

- `Core`: inputs, annotations, secrets, failure, groups, state, environment/path commands, and outputs. String outputs are literal, Boolean outputs are lowercase, and numeric outputs use invariant culture (`await Core.SetOutputAsync("count", 42)`). The numeric overload is constrained to `INumber<T>`; arbitrary objects still require explicit `JsonTypeInfo<T>`: `await Core.SetOutputAsync("report", report, ReportJsonContext.Default.Report)`. Receiving steps explicitly deserialize their string input using matching metadata. Never use outputs for large files or binary data.
- `Core.Summary`: one stable, mutable process-local buffer. Use one writer and explicitly call `WriteAsync`; it is not shared across steps.
- `Exec.RunAsync` / `GetOutputAsync`: original `ExecOptions`, output, cancellation, and nonzero-exit exceptions.
- `IO.Copy` / `Move` / `Remove` / `MakeDirectory` / `Which`: original filesystem options and behavior.
- `Glob.Create("**/*.cs")` or `Glob.Create(patterns, exclusions)`: original `Globber`, with `GlobFiles(directory)` and `Glob(directory)`.
- `GitHubApi.Context`: existing runner context, initialized only on access. `GitHubApi.CreateClient(token)` returns the original SDK client. A nonempty token is mandatory. The SDK client is not disposable; the facade adds no hidden service provider.
- `Http.CreateClient` / `CreateBasicClient` / `CreateBearerTokenClient` / `CreatePersonalAccessTokenClient`: owned clients implementing the original `IHttpClient` directly; JSON calls require metadata.
- `using var cache = Cache.CreateClient()`: `SaveCacheAsync`, `RestoreCacheAsync`, `IsFeatureAvailable`.
- `using var artifacts = Artifact.CreateClient()`: `UploadArtifactAsync`, `DownloadArtifactAsync`, `ListArtifactsAsync`, `GetArtifactAsync`, `DeleteArtifactAsync`.
- `using var tools = ToolCache.CreateClient()`: `DownloadToolAsync`, `Find`, `CacheDirAsync`, `CacheFileAsync`, `ExtractZipAsync`, `ExtractTarAsync`; `Service` exposes the original advanced interface.
- `using var attest = Attest.CreateClient()`: `AttestAsync` and `AttestProvenanceAsync` accept original options with explicit tokens. Signing requires Actions OIDC configuration and permission.

Network/resource clients support both `using` and `await using`. Each owns only its module's service graph; unrelated modules are never initialized. Finish outstanding operations before disposing; do not retain advanced service references past their owner. Tokens are never mutated or cached globally. Dispose response messages yourself.

Artifact/cache operations require the appropriate Actions runtime credentials/environment. These are not available in every ordinary shell step: configure the runner integration rather than substituting a GitHub API token. Client creation performs no permission escalation, automatic tool installation, or eager network requests. State is for phases of one action, not separate steps. `Core.SetFailed` sets the process exit code; uncaught exceptions and child-process failures remain failures.
