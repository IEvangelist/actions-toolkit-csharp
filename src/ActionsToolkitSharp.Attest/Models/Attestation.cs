// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Models;

/// <summary>
/// The result of attesting a subject. Mirrors the <c>Attestation</c> type from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/shared.types.ts">
/// <c>actions/toolkit:packages/attest/src/shared.types.ts</c></a>.
/// </summary>
public sealed class Attestation
{
    /// <summary>
    /// JSON-serialized Sigstore bundle containing the attestation, signature,
    /// signing certificate and (where available) witnessed timestamp / Rekor
    /// transparency log entry.
    /// </summary>
    public required JsonNode Bundle { get; init; }

    /// <summary>
    /// PEM-encoded signing certificate used to sign the attestation.
    /// </summary>
    public required string Certificate { get; init; }

    /// <summary>
    /// Identifier of the Rekor transparency log entry created for the
    /// attestation, when one was generated.
    /// </summary>
    public string? TlogId { get; init; }

    /// <summary>
    /// Identifier of the persisted attestation as returned by the GitHub
    /// attestations API. <see langword="null"/> when
    /// <see cref="AttestOptions.SkipWrite"/> is <see langword="true"/>.
    /// </summary>
    public string? AttestationId { get; init; }
}
