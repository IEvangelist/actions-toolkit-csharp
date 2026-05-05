// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Sigstore;

namespace ActionsToolkit.Attest.Signing;

/// <summary>
/// Default <see cref="IAttestSigner"/> that delegates to the
/// <c>mitchdenny/sigstore-dotnet</c> library
/// (<c>SigstoreSigner.AttestAsync</c>) for DSSE-wrapped in-toto signing.
/// Mirrors the upstream <c>signPayload</c> behavior from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/sign.ts">
/// <c>actions/toolkit:packages/attest/src/sign.ts</c></a>.
/// </summary>
internal sealed class SigstoreAttestSigner : IAttestSigner
{
    private readonly SigstoreSignerFactory _signerFactory;

    public SigstoreAttestSigner(SigstoreSignerFactory signerFactory)
    {
        ArgumentNullException.ThrowIfNull(signerFactory);
        _signerFactory = signerFactory;
    }

    public async Task<AttestSignResult> SignAsync(
        string inTotoStatementJson,
        SigstoreInstance instance,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inTotoStatementJson);

        var endpoints = EndpointsResolver.Resolve(instance);
        var signer = _signerFactory.Create(endpoints);
        var bundle = await signer.AttestAsync(inTotoStatementJson, cancellationToken).ConfigureAwait(false);

        var json = bundle.Serialize();
        var node = JsonNode.Parse(json)
            ?? throw new InvalidOperationException("Sigstore bundle serialization returned null JSON.");

        var certificate = SigstoreSignerFactory.ExtractLeafCertificatePem(node);
        var tlogId = ExtractTlogId(node);

        return new AttestSignResult(node, certificate, tlogId);
    }

    private static string? ExtractTlogId(JsonNode bundle)
    {
        if (bundle["verificationMaterial"] is not JsonObject vm)
        {
            return null;
        }

        if (vm["tlogEntries"] is not JsonArray entries || entries.Count == 0)
        {
            return null;
        }

        return entries[0]?["logIndex"]?.GetValue<string>();
    }
}
