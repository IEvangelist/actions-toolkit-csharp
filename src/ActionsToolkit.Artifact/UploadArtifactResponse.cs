// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Response returned from <see cref="IArtifactClient.UploadArtifactAsync"/>
/// once an artifact has been successfully created, uploaded, and finalized.
/// Mirrors upstream <c>UploadArtifactResponse</c> from
/// <c>@actions/artifact</c>.
/// </summary>
/// <param name="Id">The backend-assigned numeric identifier of the uploaded
/// artifact.</param>
/// <param name="Size">Total uploaded size in bytes (the size of the produced
/// zip archive).</param>
/// <param name="Digest">The SHA-256 digest of the uploaded archive, prefixed
/// with <c>sha256:</c>. Null only if the upload pipeline did not compute one
/// (currently always set on success).</param>
public sealed record UploadArtifactResponse(
    long Id,
    long Size,
    string? Digest = null)
{
    /// <summary>
    /// Backwards-compatible alias for <see cref="Id"/> retained for callers
    /// that referenced the Phase 3a single-property record. Will be removed
    /// in a future major release.
    /// </summary>
    [Obsolete("Use the Id property instead. This alias will be removed in a future major release.")]
    public long ArtifactId => Id;
}

