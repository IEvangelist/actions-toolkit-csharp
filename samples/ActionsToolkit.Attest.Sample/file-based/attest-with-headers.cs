// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// attest-with-headers.cs
//
// Mirrors the upstream `@actions/attest` README "Custom headers" section:
// https://github.com/actions/toolkit/tree/main/packages/attest#options
//
// Demonstrates the `Headers` option for attaching extra headers to the
// `POST /repos/{owner}/{repo}/attestations` request — for example the
// GitHub Enterprise tracing headers required by some installations.
//
// Run with:
//   GITHUB_TOKEN=ghs_xxx \
//   GITHUB_REPOSITORY=owner/repo \
//   dotnet run attest-with-headers.cs path/to/artifact

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
    : throw new ArgumentException("Usage: dotnet run attest-with-headers.cs <artifact-path>");

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
    PredicateType = "https://in-toto.io/attestation/release/v0.1",
    Predicate = JsonNode.Parse("""{ "purl": "pkg:nuget/My.Lib@1.0.0" }""")!,
    Token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!,
    Headers = new Dictionary<string, string>
    {
        ["X-Custom-Trace-Id"] = Guid.NewGuid().ToString(),
        ["X-GitHub-Enterprise-Region"] = Environment.GetEnvironmentVariable("GHE_REGION") ?? "us-east-1",
    },
});

Console.WriteLine($"Attestation id: {attestation.AttestationId}");
