// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Services;

/// <summary>
/// High-level entry point for generating GitHub artifact attestations. Mirrors
/// the <c>attest</c> and <c>attestProvenance</c> functions from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/index.ts">
/// <c>actions/toolkit:packages/attest/src/index.ts</c></a>.
/// </summary>
public interface IAttestService
{
    /// <summary>
    /// Generates an attestation for the given subject and predicate. The
    /// subject and predicate are combined into an in-toto v1 statement, which
    /// is then signed using the identified Sigstore instance and (unless
    /// <see cref="AttestOptions.SkipWrite"/>) POSTed to the GitHub
    /// attestations API.
    /// </summary>
    /// <param name="options">The options describing what to attest.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The generated <see cref="Attestation"/>.</returns>
    ValueTask<Attestation> AttestAsync(
        AttestOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an SLSA build provenance attestation for the supplied
    /// subject. The provenance predicate is hydrated from the GitHub Actions
    /// OIDC token claims.
    /// </summary>
    /// <param name="options">The options describing what to attest.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The generated <see cref="Attestation"/>.</returns>
    ValueTask<Attestation> AttestProvenanceAsync(
        AttestProvenanceOptions options,
        CancellationToken cancellationToken = default);
}
