// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Models;

/// <summary>
/// The predicate of an attestation. Mirrors the <c>Predicate</c> type from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/shared.types.ts">
/// <c>actions/toolkit:packages/attest/src/shared.types.ts</c></a>.
/// </summary>
public sealed class Predicate
{
    /// <summary>
    /// URI identifying the content type of the predicate.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Predicate parameters as a JSON object.
    /// </summary>
    public required JsonNode Params { get; init; }
}
