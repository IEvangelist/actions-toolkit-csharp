# ActionsToolkitSharp.Artifact

[![NuGet](https://img.shields.io/nuget/v/ActionsToolkitSharp.Artifact?label=ActionsToolkitSharp.Artifact&logo=nuget)](https://www.nuget.org/packages/ActionsToolkitSharp.Artifact)

An unofficial .NET SDK for GitHub Actions workflows, providing a port of the
[`@actions/artifact`](https://github.com/actions/toolkit/tree/main/packages/artifact)
JavaScript package. Use it from a custom action (or a workflow-driven .NET
process running on a GitHub Actions runner) to upload artifacts produced by
your workflow.

> [!NOTE]
> Phase 3a only ships `UploadArtifactAsync`. List, get, download, and delete
> are tracked for Phase 3b.

## Install

```bash
dotnet add package ActionsToolkitSharp.Artifact
```

## Usage

```csharp
using System.IO.Compression;
using ActionsToolkitSharp.Artifact;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsArtifact()
    .BuildServiceProvider();

var client = provider.GetRequiredService<IArtifactClient>();

using var zip = new MemoryStream();
using (var archive = new ZipArchive(zip, ZipArchiveMode.Create, leaveOpen: true))
{
    var entry = archive.CreateEntry("hello.txt");
    await using var entryStream = new StreamWriter(entry.Open());
    await entryStream.WriteAsync("hello, world");
}

zip.Position = 0;

var response = await client.UploadArtifactAsync("my-artifact", zip);
Console.WriteLine($"Uploaded artifact {response.ArtifactId}");
```

`AddGitHubActionsArtifact()` reads `ACTIONS_RESULTS_URL` and
`ACTIONS_RUNTIME_TOKEN` (set automatically by the runner) at the time the
underlying `HttpClient` is materialized; you don't need to configure them
explicitly.

## Architecture

The package decomposes the upload flow into three injectable seams:

- **`IArtifactClient`** — public façade. The DI default
  (`DefaultArtifactClient`) orchestrates create → blob PUT → finalize.
- **`IArtifactService`** — internal Twirp transport. Default issues
  `POST /twirp/github.actions.results.api.v1.ArtifactService/<Method>`
  requests against an `HttpClient` configured with the runtime token bearer.
- **`IBackendIdsProvider`** — internal accessor that reads
  `ACTIONS_RUNTIME_TOKEN` and parses the JWT `scp` claim once. Tests can
  substitute a fake. JWT parsing is hand-rolled with `JsonDocument` +
  `Base64Url` to keep the package free of `Microsoft.IdentityModel.*`
  reflection paths and AOT-clean.

The blob upload uses the no-auth `IHttpClient` from
`ActionsToolkitSharp.HttpClient` — the SAS query string carries the
credential, so we do not want to leak the GitHub bearer token to Azure.

## Errors

| Exception                          | Thrown when                                                              |
|------------------------------------|--------------------------------------------------------------------------|
| `InvalidArtifactNameException`     | `name` is null or empty.                                                 |
| `InvalidArtifactTokenException`    | `ACTIONS_RUNTIME_TOKEN` is missing or not a parseable Actions JWT.       |
| `ArtifactUploadException`          | The Twirp create / blob PUT / Twirp finalize step failed.                |
| `ArtifactNotFoundException`        | (Phase 3b) The requested artifact does not exist.                        |

## Attribution

Based on the original Node.js
[`@actions/artifact`](https://github.com/actions/toolkit/tree/main/packages/artifact)
package by GitHub. The original C# upload skeleton was contributed by
[@js6pak](https://github.com/js6pak) in
[#4](https://github.com/IEvangelist/dotnet-github-actions-sdk/pull/4) and is
preserved as the first commit on the adoption branch.
