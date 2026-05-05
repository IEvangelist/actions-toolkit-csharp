// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Models;

/// <summary>
/// The subject of an attestation. Mirrors the <c>Subject</c> type from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/shared.types.ts">
/// <c>actions/toolkit:packages/attest/src/shared.types.ts</c></a>.
/// </summary>
public sealed class Subject
{
    /// <summary>
    /// Name of the subject.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Digests of the subject. Map of digest algorithms (e.g. <c>sha256</c>) to
    /// their hex-encoded values.
    /// </summary>
    [JsonPropertyName("digest")]
    public required IReadOnlyDictionary<string, string> Digest { get; init; }
}
