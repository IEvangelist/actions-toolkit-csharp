// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Services;

/// <summary>
/// Builds an SLSA build provenance <see cref="Predicate"/> populated with
/// metadata pulled from the GitHub Actions OIDC token. Mirrors the
/// <c>buildSLSAProvenancePredicate</c> function from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/provenance.ts">
/// <c>actions/toolkit:packages/attest/src/provenance.ts</c></a>.
/// </summary>
public interface IProvenancePredicateBuilder
{
    /// <summary>
    /// Builds the SLSA v1 provenance predicate.
    /// </summary>
    /// <param name="issuer">Optional override for the OIDC issuer URL.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<Predicate> BuildAsync(string? issuer, CancellationToken cancellationToken = default);
}
