// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// attest-provenance.cs
//
// Mirrors the upstream `@actions/attest` README "attestProvenance" section:
// https://github.com/actions/toolkit/tree/main/packages/attest#attestprovenance
//
// Generates a SLSA v1 build provenance attestation for an artifact built by
// the current GitHub Actions workflow run. The OIDC claims for the running
// job are decoded into the predicate parameters.
//
// Run with:
//   GITHUB_TOKEN=ghs_xxx \
//   ACTIONS_ID_TOKEN_REQUEST_URL=https://… \
//   ACTIONS_ID_TOKEN_REQUEST_TOKEN=… \
//   GITHUB_REPOSITORY=owner/repo \
//   GITHUB_SERVER_URL=https://github.com \
//   dotnet run attest-provenance.cs path/to/artifact

#:package ActionsToolkit.Attest@*
#:package ActionsToolkit.Octokit@*
#:package Microsoft.Extensions.DependencyInjection@*
#:package Microsoft.Extensions.Http@*

using System.Security.Cryptography;
using System.Text.Json.Nodes;

using ActionsToolkit.Attest;
using ActionsToolkit.Attest.Models;
using ActionsToolkit.Attest.Services;
using ActionsToolkit.Octokit.Extensions;
using Microsoft.Extensions.DependencyInjection;

var artifactPath = args is [var path, ..]
    ? path
    : throw new ArgumentException("Usage: dotnet run attest-provenance.cs <artifact-path>");

await using var stream = File.OpenRead(artifactPath);
var digest = Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLowerInvariant();

using var provider = new ServiceCollection()
    .AddGitHubClientServices(Environment.GetEnvironmentVariable("GITHUB_TOKEN")!)
    .AddAttestServices()
    .BuildServiceProvider();

var attest = provider.GetRequiredService<IAttestService>();

var attestation = await attest.AttestProvenanceAsync(new AttestProvenanceOptions
{
    SubjectName = Path.GetFileName(artifactPath),
    SubjectDigest = new Dictionary<string, string> { ["sha256"] = digest },
    Token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!,
});

Console.WriteLine($"Attestation id: {attestation.AttestationId}");
Console.WriteLine($"Bundle:         {attestation.Bundle.ToJsonString()}");
