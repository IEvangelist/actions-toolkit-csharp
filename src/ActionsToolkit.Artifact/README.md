# ActionsToolkit.Artifact

[![NuGet](https://img.shields.io/nuget/v/ActionsToolkit.Artifact?label=ActionsToolkit.Artifact&logo=nuget)](https://www.nuget.org/packages/ActionsToolkit.Artifact)

An unofficial .NET SDK for GitHub Actions workflows, providing a port of the
[`@actions/artifact`](https://github.com/actions/toolkit/tree/main/packages/artifact)
JavaScript package. Use it from a custom action (or a workflow-driven .NET
process running on a GitHub Actions runner) to upload, list, get, download,
and delete workflow run artifacts.

## Install

```xml
<PackageReference Include="ActionsToolkit.Artifact" Version="[Version]" />
```

Or use the [`dotnet add package`](https://learn.microsoft.com/dotnet/core/tools/dotnet-add-package) .NET CLI command:

```bash
dotnet add package ActionsToolkit.Artifact
```

## Get the `IArtifactClient` instance

Register the services with an `IServiceCollection` instance by calling
`AddGitHubActionsArtifact()`, and consuming code can require the
`IArtifactClient` via constructor dependency injection.

```csharp
using ActionsToolkit.Artifact;
using ActionsToolkit.Artifact.Extensions;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsArtifact()
    .BuildServiceProvider();

var client = provider.GetRequiredService<IArtifactClient>();
```

`AddGitHubActionsArtifact()` reads `ACTIONS_RESULTS_URL`,
`ACTIONS_RUNTIME_TOKEN`, `GITHUB_TOKEN`, and `GITHUB_API_URL` (set
automatically by the runner) at the time each `HttpClient` is materialized;
you don't need to configure them explicitly.

## Basic usage

The client mirrors the JavaScript [`ArtifactClient`](https://github.com/actions/toolkit/blob/main/packages/artifact/docs/generated/classes/internal_client.DefaultArtifactClient.md)
surface with five idiomatic .NET methods returning `ValueTask<T>`.

### Upload an artifact

```csharp
string[] files = { "logs/build.log", "logs/test.trx" };

UploadArtifactResponse response = await client.UploadArtifactAsync(
    name: "build-logs",
    files: files,
    rootDirectory: "logs");

Console.WriteLine($"Uploaded artifact {response.Id} ({response.Size} bytes, digest {response.Digest})");
```

`UploadArtifactOptions` controls retention (`RetentionDays`) and ZIP
compression (`CompressionLevel`, 0-9 mapped onto .NET
[`CompressionLevel`](https://learn.microsoft.com/dotnet/api/system.io.compression.compressionlevel)
values: 0 → `NoCompression`, 1-3 → `Fastest`, 4-6 → `Optimal`, 7-9 →
`SmallestSize`).

### List artifacts in a run

```csharp
ListArtifactsResponse list = await client.ListArtifactsAsync();
foreach (var artifact in list.Artifacts)
{
    Console.WriteLine($"{artifact.Id}: {artifact.Name} ({artifact.Size} bytes)");
}
```

`ListArtifactsOptions.LatestOnly` filters out re-uploads of an artifact with
the same name, keeping the most recent only. `FindOptions.FindBy` lets you
target a different repository / run / token combination.

### Get a single artifact by name

```csharp
GetArtifactResponse result = await client.GetArtifactAsync("build-logs");
Console.WriteLine($"Found {result.Artifact.Id} created {result.Artifact.CreatedAt:o}");
```

### Download an artifact

```csharp
DownloadArtifactResponse download = await client.DownloadArtifactAsync(
    artifactId: result.Artifact.Id,
    options: new DownloadArtifactOptions { Path = "./extracted" });

Console.WriteLine($"Extracted to {download.DownloadPath}");
```

`DownloadArtifactOptions.SkipDecompress = true` keeps the raw `.zip` on
disk instead of expanding it.

### Delete an artifact

```csharp
DeleteArtifactResponse deleted = await client.DeleteArtifactAsync("build-logs");
Console.WriteLine($"Deleted artifact {deleted.Id}");
```

## Architecture

The package decomposes the artifact flows into injectable seams:

- **`IArtifactClient`** — the public façade. The DI default
  (`DefaultArtifactClient`) routes upload through the Twirp pipeline and
  list/get/download/delete through the public REST API at
  `https://api.github.com/`.
- **`IArtifactService`** *(internal)* — Twirp transport. Issues
  `POST /twirp/github.actions.results.api.v1.ArtifactService/<Method>`
  requests with the runtime token bearer.
- **`IPublicArtifactsApi`** *(internal)* — Public REST transport for the
  list / get / download / delete operations, using `GITHUB_TOKEN`.
- **`IBackendIdsProvider`** *(internal)* — reads `ACTIONS_RUNTIME_TOKEN` and
  parses the JWT `scp` claim once. JWT parsing is hand-rolled with
  `JsonDocument` + `Base64Url` to keep the package free of
  `Microsoft.IdentityModel.*` reflection paths and AOT-clean.

The package registers three named `HttpClient` instances:

| Name                                       | Purpose                                                              |
|--------------------------------------------|----------------------------------------------------------------------|
| `ActionsToolkit.Artifact`             | Twirp transport (bearer auth from `ACTIONS_RUNTIME_TOKEN`).          |
| `ActionsToolkit.Artifact.PublicApi`   | REST API at `https://api.github.com/`, no auto-redirect.             |
| `ActionsToolkit.Artifact.Blob`        | Bare client for blob upload/download via signed URLs (no auth).      |

`AllowAutoRedirect=false` on the public REST client lets `DownloadArtifactAsync`
extract the signed blob URL from the `302` `Location` header *before*
sending the GitHub bearer token to Azure.

## GHES support

The list / get / download / delete operations require GitHub.com (or
`*.ghe.com`); they throw `GhesNotSupportedException` on a GHES host.
Uploads always go through the Twirp endpoint and continue to work on GHES.

## Errors

| Exception                          | Thrown when                                                              |
|------------------------------------|--------------------------------------------------------------------------|
| `InvalidArtifactNameException`     | `name` is null/empty or contains a forbidden character.                  |
| `InvalidArtifactTokenException`    | `ACTIONS_RUNTIME_TOKEN` is missing or not a parseable Actions JWT.       |
| `FilesNotFoundException`           | None of the supplied upload paths exist on disk.                         |
| `ArtifactUploadException`          | The Twirp create / blob PUT / Twirp finalize step failed.                |
| `ArtifactNotFoundException`        | The requested artifact does not exist (list / get / delete).             |
| `ArtifactNetworkException`         | A network call to the public REST API or blob storage failed.            |
| `ArtifactUsageException`           | Required runner environment variables (e.g., `GITHUB_TOKEN`) are missing.|
| `InvalidArtifactResponseException` | The Twirp / REST server returned a malformed response body.              |
| `GhesNotSupportedException`        | A list / get / download / delete call was made on a GHES host.           |

## Attribution

Based on the original Node.js
[`@actions/artifact`](https://github.com/actions/toolkit/tree/main/packages/artifact)
package by GitHub. The original C# upload skeleton was contributed by
[@js6pak](https://github.com/js6pak) in
[#4](https://github.com/IEvangelist/dotnet-github-actions-sdk/pull/4) and
is preserved as the first commit on the adoption branch.
