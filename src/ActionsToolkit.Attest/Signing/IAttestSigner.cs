// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Signing;

/// <summary>
/// Internal abstraction over <c>Sigstore.SigstoreSigner.AttestAsync</c> so that
/// the attest pipeline can be unit-tested without contacting real Fulcio /
/// Rekor / TSA endpoints. The default implementation
/// (<see cref="SigstoreAttestSigner"/>) delegates to the
/// <c>mitchdenny/sigstore-dotnet</c> library.
/// </summary>
public interface IAttestSigner
{
    /// <summary>
    /// Signs the supplied in-toto v1 statement payload as a DSSE envelope and
    /// returns the resulting Sigstore bundle.
    /// </summary>
    /// <param name="inTotoStatementJson">The serialized in-toto v1 statement
    /// to sign.</param>
    /// <param name="instance">The Sigstore instance whose Fulcio / Rekor / TSA
    /// endpoints should be targeted.</param>
    /// <param name="cancellationToken">Token to cancel the sign.</param>
    /// <returns>The result of the sign — bundle JSON, leaf certificate (PEM),
    /// and (when present) the Rekor transparency log entry id.</returns>
    Task<AttestSignResult> SignAsync(
        string inTotoStatementJson,
        SigstoreInstance instance,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The byproducts of <see cref="IAttestSigner.SignAsync"/>: the Sigstore
/// bundle as a <see cref="JsonNode"/>, the PEM-encoded signing certificate,
/// and (optionally) the transparency log id.
/// </summary>
/// <param name="Bundle">The Sigstore bundle as a JSON object.</param>
/// <param name="Certificate">The PEM-encoded leaf signing certificate.</param>
/// <param name="TlogId">The Rekor transparency log entry id, when one was
/// recorded; otherwise <see langword="null"/>.</param>
public sealed record AttestSignResult(JsonNode Bundle, string Certificate, string? TlogId);
