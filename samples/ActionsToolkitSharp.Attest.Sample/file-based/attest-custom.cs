// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// attest-custom.cs
//
// Mirrors the upstream `@actions/attest` README "attest" section:
// https://github.com/actions/toolkit/tree/main/packages/attest#attest
//
// Generates an attestation with a caller-supplied predicate type and body —
// for example a custom `https://my.example.com/predicate/v1` predicate or
// a published spec like the SLSA "verification summary" predicate.
//
// Run with:
//   GITHUB_TOKEN=ghs_xxx \
//   ACTIONS_ID_TOKEN_REQUEST_URL=… \
//   ACTIONS_ID_TOKEN_REQUEST_TOKEN=… \
//   GITHUB_REPOSITORY=owner/repo \
//   dotnet run attest-custom.cs path/to/artifact

#:package ActionsToolkitSharp.Attest@*
#:package ActionsToolkitSharp.Octokit@*
#:package Microsoft.Extensions.DependencyInjection@*
#:package Microsoft.Extensions.Http@*

using System.Security.Cryptography;
using System.Text.Json.Nodes;

using ActionsToolkitSharp.Attest;
using ActionsToolkitSharp.Attest.Models;
using ActionsToolkitSharp.Attest.Services;
using ActionsToolkitSharp.Octokit.Extensions;
using Microsoft.Extensions.DependencyInjection;

var artifactPath = args is [var path, ..]
    ? path
    : throw new ArgumentException("Usage: dotnet run attest-custom.cs <artifact-path>");

await using var stream = File.OpenRead(artifactPath);
var digest = Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLowerInvariant();

using var provider = new ServiceCollection()
    .AddGitHubClientServices(Environment.GetEnvironmentVariable("GITHUB_TOKEN")!)
    .AddAttestServices()
    .BuildServiceProvider();

var attest = provider.GetRequiredService<IAttestService>();

var attestation = await attest.AttestAsync(new AttestOptions
{
    SubjectName = Path.GetFileName(artifactPath),
    SubjectDigest = new Dictionary<string, string> { ["sha256"] = digest },
    PredicateType = "https://my.example.com/predicate/v1",
    Predicate = JsonNode.Parse("""{ "key": "value" }""")!,
    Token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!,
});

Console.WriteLine($"Attestation id: {attestation.AttestationId}");
