// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Models;

/// <summary>
/// Options for <see cref="Services.IAttestService.AttestAsync"/>. Mirrors the
/// <c>AttestOptions</c> shape from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/attest.ts">
/// <c>actions/toolkit:packages/attest/src/attest.ts</c></a>.
/// </summary>
public sealed class AttestOptions
{
    /// <summary>
    /// Deprecated: use <see cref="Subjects"/>. Convenience for a single subject.
    /// </summary>
    public string? SubjectName { get; init; }

    /// <summary>
    /// Deprecated: use <see cref="Subjects"/>. Companion to
    /// <see cref="SubjectName"/>; map of digest algorithm to hex value.
    /// </summary>
    public IReadOnlyDictionary<string, string>? SubjectDigest { get; init; }

    /// <summary>
    /// The collection of subjects to attest.
    /// </summary>
    public IReadOnlyList<Subject>? Subjects { get; init; }

    /// <summary>
    /// URI identifying the content type of the predicate being attested.
    /// </summary>
    public required string PredicateType { get; init; }

    /// <summary>
    /// The predicate body to attest, as a JSON object.
    /// </summary>
    public required JsonNode Predicate { get; init; }

    /// <summary>
    /// GitHub token used to write the attestation to the repository's
    /// attestations endpoint.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The Sigstore instance to use for signing. When unset, the default is
    /// <see cref="SigstoreInstance.PublicGood"/>.
    /// </summary>
    public SigstoreInstance? Sigstore { get; init; }

    /// <summary>
    /// Additional HTTP headers to include in the request to the GitHub
    /// attestations API.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the produced attestation is not POSTed to
    /// the GitHub attestations endpoint. The bundle is still returned.
    /// </summary>
    public bool SkipWrite { get; init; }
}
