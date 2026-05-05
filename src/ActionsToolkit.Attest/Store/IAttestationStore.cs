// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Store;

/// <summary>
/// Persists a signed Sigstore bundle to a backing storage system. The default
/// implementation (<see cref="GitHubAttestationStore"/>) POSTs the bundle to
/// <c>POST /repos/{owner}/{repo}/attestations</c> on the GitHub REST API.
/// Mirrors the <c>writeAttestation</c> function from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/store.ts">
/// <c>actions/toolkit:packages/attest/src/store.ts</c></a>.
/// </summary>
public interface IAttestationStore
{
    /// <summary>
    /// Persists the supplied <paramref name="bundle"/> and returns the
    /// resulting attestation id (or <see langword="null"/> if the backing
    /// store does not return one).
    /// </summary>
    /// <param name="bundle">The Sigstore bundle JSON.</param>
    /// <param name="token">The GitHub token used for write authorization.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The persisted attestation id, when one is returned.</returns>
    Task<string?> WriteAsync(
        JsonNode bundle,
        string token,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}
