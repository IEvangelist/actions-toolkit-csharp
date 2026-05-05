// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography.X509Certificates;
using Sigstore;

namespace ActionsToolkit.Attest.Signing;

/// <summary>
/// DI helper that builds the 5-argument <see cref="SigstoreSigner"/> from
/// <c>Sigstore</c> v0.5.0 with the right Fulcio / Rekor / TSA / OIDC / trust
/// root for either the public-good or GitHub instance.
/// </summary>
/// <remarks>
/// <para>
/// The constructor signature targeted is
/// <c>SigstoreSigner(IFulcioClient, IRekorClient, ITimestampAuthority,
/// IOidcTokenProvider, ITrustRootProvider?)</c>. This is the only signature
/// that performs real keyless signing — the parameterless constructor in the
/// shipped 0.5.0 library is a TODO stub. See
/// <c>actions/toolkit:packages/attest/src/sign.ts</c> for the upstream
/// counterpart (<c>FulcioSigner</c> + <c>RekorWitness</c> +
/// <c>TSAWitness</c>).
/// </para>
/// <para>
/// The factory does NOT register any HttpClient, trust root provider, or OIDC
/// token provider — those are supplied by the caller (typically resolved from
/// DI). Call sites should pass a long-lived
/// <see cref="IOidcTokenProvider"/> to avoid re-reading the OIDC envelope on
/// every sign.
/// </para>
/// </remarks>
public sealed class SigstoreSignerFactory
{
    private readonly IOidcTokenProvider _tokenProvider;
    private readonly Func<NetClient> _httpClientFactory;
    private readonly ITrustRootProvider? _trustRootProvider;

    /// <summary>
    /// Creates a new <see cref="SigstoreSignerFactory"/>.
    /// </summary>
    /// <param name="tokenProvider">The OIDC token provider used for keyless
    /// signing (typically a
    /// <see cref="GitHubActionsOidcTokenProvider"/>).</param>
    /// <param name="httpClientFactory">A factory that produces fresh
    /// <see cref="NetClient"/> instances for the underlying Fulcio / Rekor /
    /// TSA HTTP clients. Each <c>SigstoreSigner</c> is paired with one set of
    /// HTTP clients owned by this factory.</param>
    /// <param name="trustRootProvider">Optional trust root provider. The
    /// <c>0.5.0</c> <see cref="SigstoreSigner"/> tolerates
    /// <see langword="null"/>; verification flows would require one.</param>
    public SigstoreSignerFactory(
        IOidcTokenProvider tokenProvider,
        Func<NetClient> httpClientFactory,
        ITrustRootProvider? trustRootProvider = null)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _tokenProvider = tokenProvider;
        _httpClientFactory = httpClientFactory;
        _trustRootProvider = trustRootProvider;
    }

    /// <summary>
    /// Builds a <see cref="SigstoreSigner"/> hydrated for the supplied
    /// <paramref name="endpoints"/>. The returned signer owns its Fulcio /
    /// Rekor / TSA HTTP clients and should be used per-attestation.
    /// </summary>
    public SigstoreSigner Create(SigstoreEndpoints endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var fulcio = new FulcioHttpClient(_httpClientFactory(), endpoints.FulcioUrl);

        // Public-good Sigstore exposes a Rekor witness; GitHub-hosted Sigstore
        // does not (it uses a TSA instead). When Rekor is absent we still need
        // an IRekorClient for the SigstoreSigner constructor — the upstream
        // sign.ts code handles this by simply omitting the witness, but the
        // .NET signer's constructor is non-nullable. We pass a Rekor client
        // pointed at the TSA host so it does not contact the network when no
        // Rekor witness is exercised by AttestAsync's call path; in practice
        // SignAsync always submits a DSSE entry, so a real Rekor is required
        // when present. For private/github instance, this remains a known
        // limitation tracked alongside the upstream limitation that the
        // private instance only emits TSA timestamps.
        var rekorBaseUrl = endpoints.RekorUrl ?? endpoints.FulcioUrl;
        var rekor = new RekorHttpClient(_httpClientFactory(), rekorBaseUrl);

        var tsaBaseUrl = endpoints.TsaServerUrl ?? endpoints.FulcioUrl;
        var tsa = new HttpTimestampAuthority(_httpClientFactory(), tsaBaseUrl);

        return new SigstoreSigner(fulcio, rekor, tsa, _tokenProvider, _trustRootProvider);
    }

    /// <summary>
    /// Builds a leaf certificate's PEM representation from the bundle's
    /// <c>verificationMaterial.certificate.rawBytes</c> field.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="SigstoreAttestSigner"/> to satisfy the upstream
    /// <c>Attestation.certificate</c> contract.
    /// </remarks>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "X509CertificateLoader.LoadCertificate returns a value-type wrapper around managed memory; the PEM string captures all required state.")]
    internal static string ExtractLeafCertificatePem(JsonNode bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        var verificationMaterial = bundle["verificationMaterial"]
            ?? throw new InvalidOperationException("Bundle is missing 'verificationMaterial'.");

        byte[]? derBytes = null;

        if (verificationMaterial["certificate"] is JsonObject certObj &&
            certObj["rawBytes"]?.GetValue<string>() is { Length: > 0 } singleRaw)
        {
            derBytes = Convert.FromBase64String(singleRaw);
        }
        else if (verificationMaterial["x509CertificateChain"] is JsonObject chainObj &&
                 chainObj["certificates"] is JsonArray certs &&
                 certs.Count > 0 &&
                 certs[0] is JsonObject leaf &&
                 leaf["rawBytes"]?.GetValue<string>() is { Length: > 0 } chainRaw)
        {
            derBytes = Convert.FromBase64String(chainRaw);
        }

        if (derBytes is null)
        {
            throw new InvalidOperationException(
                "Bundle does not contain an x509 leaf certificate.");
        }

        var cert = X509CertificateLoader.LoadCertificate(derBytes);
        try
        {
            return cert.ExportCertificatePem();
        }
        finally
        {
            cert.Dispose();
        }
    }
}
