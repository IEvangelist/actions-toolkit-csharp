// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Models;

/// <summary>
/// The endpoint URLs (Fulcio, Rekor, TSA) used by Sigstore signing for a
/// chosen <see cref="SigstoreInstance"/>. Mirrors the <c>Endpoints</c> type
/// from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/endpoints.ts">
/// <c>actions/toolkit:packages/attest/src/endpoints.ts</c></a>.
/// </summary>
public sealed class SigstoreEndpoints
{
    /// <summary>
    /// The Fulcio certificate authority base URL.
    /// </summary>
    public required Uri FulcioUrl { get; init; }

    /// <summary>
    /// The Rekor transparency log base URL.
    /// </summary>
    /// <remarks>
    /// Set for the public-good instance only; the private GitHub instance does
    /// not currently expose a Rekor witness.
    /// </remarks>
    public Uri? RekorUrl { get; init; }

    /// <summary>
    /// The RFC 3161 Time Stamping Authority base URL.
    /// </summary>
    /// <remarks>
    /// Set for the GitHub instance only; the public-good instance currently
    /// uses Rekor as its sole witness.
    /// </remarks>
    public Uri? TsaServerUrl { get; init; }
}
