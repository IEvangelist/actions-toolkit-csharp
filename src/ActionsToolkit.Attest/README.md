# `ActionsToolkit.Attest`

The .NET equivalent of GitHub's
[`@actions/attest`](https://github.com/actions/toolkit/tree/main/packages/attest)
package — generate signed
[in-toto](https://github.com/in-toto/attestation/tree/main/spec/v1)
attestations for your build artifacts and persist them via the GitHub
attestations API.

This package is a port of the upstream JavaScript code under the same
[Apache-2.0 license](https://github.com/actions/toolkit/blob/main/LICENSE.md).
Cryptographic signing is delegated to
[`Sigstore`](https://www.nuget.org/packages/Sigstore/) v0.5.0
([mitchdenny/sigstore-dotnet](https://github.com/mitchdenny/sigstore-dotnet)) —
a pure .NET, AOT-compatible Sigstore client.

## Install

```bash
dotnet add package ActionsToolkit.Attest
```

`Sigstore` is pinned via this repository's central package management, so no
explicit version is required when consuming the SDK from a project that already
uses `Directory.Packages.props`.

## Register services

```csharp
using ActionsToolkit.Attest;
using ActionsToolkit.Octokit.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddGitHubClientServices(token: Environment.GetEnvironmentVariable("GITHUB_TOKEN")!)
    .AddAttestServices()
    .BuildServiceProvider();

var attest = services.GetRequiredService<IAttestService>();
```

## Attest a custom predicate

```csharp
using System.Text.Json.Nodes;

var attestation = await attest.AttestAsync(new AttestOptions
{
    Subjects =
    [
        new Subject
        {
            Name = "my-artifact-name",
            Digest = new Dictionary<string, string>
            {
                ["sha256"] = "36ab4667…",
            },
        },
    ],
    PredicateType = "https://in-toto.io/attestation/release",
    Predicate = JsonNode.Parse("""{ "purl": "pkg:nuget/My.Lib@1.0.0" }""")!,
    Token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!,
});

Console.WriteLine($"Attestation id: {attestation.AttestationId}");
Console.WriteLine($"Bundle:         {attestation.Bundle.ToJsonString()}");
```

## Attest SLSA build provenance

```csharp
var attestation = await attest.AttestProvenanceAsync(new AttestProvenanceOptions
{
    SubjectName = "my-artifact-name",
    SubjectDigest = new Dictionary<string, string>
    {
        ["sha256"] = "36ab4667…",
    },
    Token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!,
});
```

The provenance variant builds an
[SLSA v1 build provenance predicate](https://slsa.dev/spec/v1.0/provenance)
populated with the
[`github-actions-buildtypes/workflow/v1`](https://github.com/slsa-framework/github-actions-buildtypes/tree/main/workflow/v1)
external/internal parameters from the per-job OIDC token.

## Sigstore instances

The package supports both Sigstore endpoints exposed by the upstream JS
package:

| `Sigstore` value | Fulcio | Rekor / TSA |
|---|---|---|
| `SigstoreInstance.PublicGood` | `fulcio.sigstore.dev` | `rekor.sigstore.dev` |
| `SigstoreInstance.GitHub` | `fulcio.{githubapp.com\|*.ghe.com}` | `timestamp.{githubapp.com\|*.ghe.com}` |

Set `AttestOptions.Sigstore` (or `AttestProvenanceOptions.Sigstore`)
explicitly. Defaults to `PublicGood`.

## Attribution

This package ports the source of
[`@actions/attest`](https://github.com/actions/toolkit/tree/main/packages/attest),
licensed under [Apache-2.0](https://github.com/actions/toolkit/blob/main/LICENSE.md).
The cryptographic signing path is delegated to
[`Sigstore`](https://github.com/mitchdenny/sigstore-dotnet) v0.5.0 by Mitch
Denny ([MIT](https://github.com/mitchdenny/sigstore-dotnet/blob/main/LICENSE)).
