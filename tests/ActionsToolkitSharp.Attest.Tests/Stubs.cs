// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Tests;

internal sealed class StubAttestSigner : IAttestSigner
{
    private readonly Func<string, SigstoreInstance, AttestSignResult> _impl;

    public StubAttestSigner(Func<string, SigstoreInstance, AttestSignResult>? impl = null)
    {
        _impl = impl ?? DefaultImpl;
    }

    public string? LastStatementJson { get; private set; }

    public SigstoreInstance LastInstance { get; private set; }

    public Task<AttestSignResult> SignAsync(
        string inTotoStatementJson,
        SigstoreInstance instance,
        CancellationToken cancellationToken = default)
    {
        LastStatementJson = inTotoStatementJson;
        LastInstance = instance;
        return Task.FromResult(_impl(inTotoStatementJson, instance));
    }

    private static AttestSignResult DefaultImpl(string statement, SigstoreInstance instance)
    {
        var bundle = JsonNode.Parse($$"""
        {
          "mediaType": "application/vnd.dev.sigstore.bundle.v0.3+json",
          "instance": "{{instance}}"
        }
        """)!;

        return new AttestSignResult(bundle, "-----BEGIN CERTIFICATE-----\nFAKE\n-----END CERTIFICATE-----", "1");
    }
}

internal sealed class StubAttestationStore : IAttestationStore
{
    private readonly string? _id;

    public StubAttestationStore(string? id = "999")
    {
        _id = id;
    }

    public JsonNode? LastBundle { get; private set; }
    public string? LastToken { get; private set; }
    public IReadOnlyDictionary<string, string>? LastHeaders { get; private set; }
    public int CallCount { get; private set; }

    public Task<string?> WriteAsync(
        JsonNode bundle,
        string token,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastBundle = bundle;
        LastToken = token;
        LastHeaders = headers;
        return Task.FromResult<string?>(_id);
    }
}

internal sealed class StubProvenancePredicateBuilder : IProvenancePredicateBuilder
{
    private readonly Predicate _predicate;

    public StubProvenancePredicateBuilder(Predicate? predicate = null)
    {
        _predicate = predicate ?? new Predicate
        {
            Type = "https://slsa.dev/provenance/v1",
            Params = JsonNode.Parse("""{ "buildDefinition": { "buildType": "https://example.test/build/v1" } }""")!,
        };
    }

    public Task<Predicate> BuildAsync(string? issuer, CancellationToken cancellationToken = default) =>
        Task.FromResult(_predicate);
}
