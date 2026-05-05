// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Models;

/// <summary>
/// Options for
/// <see cref="Services.IAttestService.AttestProvenanceAsync"/>. Mirrors the
/// <c>AttestProvenanceOptions</c> type from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/provenance.ts">
/// <c>actions/toolkit:packages/attest/src/provenance.ts</c></a>.
/// </summary>
public sealed class AttestProvenanceOptions
{
    /// <summary>
    /// Deprecated: use <see cref="Subjects"/>.
    /// </summary>
    public string? SubjectName { get; init; }

    /// <summary>
    /// Deprecated: use <see cref="Subjects"/>.
    /// </summary>
    public IReadOnlyDictionary<string, string>? SubjectDigest { get; init; }

    /// <summary>
    /// The collection of subjects to attest.
    /// </summary>
    public IReadOnlyList<Subject>? Subjects { get; init; }

    /// <summary>
    /// GitHub token used to write the attestation.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The Sigstore instance to use for signing.
    /// </summary>
    public SigstoreInstance? Sigstore { get; init; }

    /// <summary>
    /// Additional HTTP headers to include in the request to the GitHub
    /// attestations API.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the produced attestation is not POSTed to
    /// the GitHub attestations endpoint.
    /// </summary>
    public bool SkipWrite { get; init; }

    /// <summary>
    /// The OIDC issuer URL responsible for minting the GitHub Actions ID
    /// token. Defaults to the issuer derived from
    /// <c>GITHUB_SERVER_URL</c> (typically
    /// <c>https://token.actions.githubusercontent.com</c>).
    /// </summary>
    public string? Issuer { get; init; }
}
