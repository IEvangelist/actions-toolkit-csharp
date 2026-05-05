// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// Driver for the Attest sample. Exercises the in-process DI surface using a
// stub IAttestSigner and IAttestationStore so the sample can be run locally
// without contacting Sigstore or GitHub. For real-world usage see the
// `file-based/` examples or the `usage.yml` workflow snippet.

var artifactPath = args is [var path, ..] ? path : "sample-artifact.txt";

if (!File.Exists(artifactPath))
{
    await File.WriteAllTextAsync(artifactPath, "hello, attestations");
}

await using var stream = File.OpenRead(artifactPath);
var digestHex = Convert.ToHexString(await SHA256.HashDataAsync(stream).ConfigureAwait(false)).ToLowerInvariant();

var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "fake-token";

var services = new ServiceCollection();

services.AddSingleton<IAttestSigner, OfflineAttestSigner>();
services.AddSingleton<IAttestationStore, OfflineAttestationStore>();

services
    .AddGitHubActionsCore()
    .AddGitHubClientServices(token)
    .AddAttestServices();

using var provider = services.BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();
var attest = provider.GetRequiredService<IAttestService>();

try
{
    var attestation = await attest.AttestAsync(new AttestOptions
    {
        SubjectName = Path.GetFileName(artifactPath),
        SubjectDigest = new Dictionary<string, string> { ["sha256"] = digestHex },
        PredicateType = "https://example.test/sample/v1",
        Predicate = JsonNode.Parse("""{ "sample": true }""")!,
        Token = token,
        SkipWrite = false,
    }).ConfigureAwait(false);

    core.WriteInfo($"Attestation id: {attestation.AttestationId}");
    core.WriteInfo($"Bundle:         {attestation.Bundle.ToJsonString()}");
}
catch (Exception ex)
{
    core.SetFailed(ex.ToString());
}

internal sealed class OfflineAttestSigner : IAttestSigner
{
    public Task<AttestSignResult> SignAsync(string statement, SigstoreInstance instance, CancellationToken ct = default)
    {
        var bundle = JsonNode.Parse("""{ "mediaType": "application/vnd.dev.sigstore.bundle.v0.3+json", "offline": true }""")!;
        return Task.FromResult(new AttestSignResult(bundle, "-----BEGIN CERTIFICATE-----\nFAKE\n-----END CERTIFICATE-----", "0"));
    }
}

internal sealed class OfflineAttestationStore : IAttestationStore
{
    public Task<string?> WriteAsync(JsonNode bundle, string token, IReadOnlyDictionary<string, string>? headers = null, CancellationToken ct = default) =>
        Task.FromResult<string?>("offline-attestation-id");
}
