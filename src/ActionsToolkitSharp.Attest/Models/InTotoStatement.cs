// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Models;

/// <summary>
/// The in-toto v1 statement that wraps the subject and predicate. Mirrors
/// <a href="https://github.com/in-toto/attestation/blob/main/spec/v1/statement.md">
/// the in-toto v1 statement spec</a> and the <c>InTotoStatement</c> type from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/intoto.ts">
/// <c>actions/toolkit:packages/attest/src/intoto.ts</c></a>.
/// </summary>
public sealed class InTotoStatement
{
    /// <summary>
    /// The statement type URI. Always
    /// <c>https://in-toto.io/Statement/v1</c>.
    /// </summary>
    [JsonPropertyName("_type")]
    public required string Type { get; init; }

    /// <summary>
    /// The collection of subjects covered by this statement.
    /// </summary>
    [JsonPropertyName("subject")]
    public required IReadOnlyList<Subject> Subject { get; init; }

    /// <summary>
    /// URI identifying the content type of the predicate.
    /// </summary>
    [JsonPropertyName("predicateType")]
    public required string PredicateType { get; init; }

    /// <summary>
    /// The predicate body.
    /// </summary>
    [JsonPropertyName("predicate")]
    public required JsonNode Predicate { get; init; }
}
