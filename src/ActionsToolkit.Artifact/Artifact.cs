// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Metadata for an Actions workflow artifact returned by list, get, and
/// upload operations. Mirrors the upstream <c>Artifact</c> interface from
/// <c>@actions/artifact</c>.
/// </summary>
/// <param name="Name">The artifact name.</param>
/// <param name="Id">The backend-assigned artifact identifier.</param>
/// <param name="Size">The artifact size in bytes.</param>
/// <param name="CreatedAt">When the artifact was created. May be unset for
/// artifacts produced by older toolkit versions.</param>
/// <param name="Digest">The SHA-256 digest computed at upload time. May be
/// unset for artifacts uploaded prior to digest support.</param>
public sealed record Artifact(
    string Name,
    long Id,
    long Size,
    DateTimeOffset? CreatedAt = null,
    string? Digest = null);
