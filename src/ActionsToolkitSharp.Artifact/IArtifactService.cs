// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// Internal abstraction over the Twirp-over-HTTP RPCs exposed by the GitHub
/// Actions results service (<c>github.actions.results.api.v1.ArtifactService</c>).
/// All five methods correspond directly to upstream operations; only
/// <see cref="CreateArtifactAsync"/> and <see cref="FinalizeArtifactAsync"/>
/// are wired into a public API by Phase 3a — list, get-signed-url, and delete
/// are implemented but not yet surfaced through <see cref="IArtifactClient"/>
/// (Phase 3b will add those).
/// </summary>
internal interface IArtifactService
{
    Task<CreateArtifactResponse> CreateArtifactAsync(
        CreateArtifactRequest request,
        CancellationToken cancellationToken = default);

    Task<FinalizeArtifactResponse> FinalizeArtifactAsync(
        FinalizeArtifactRequest request,
        CancellationToken cancellationToken = default);

    Task<ListArtifactsResponse> ListArtifactsAsync(
        ListArtifactsRequest request,
        CancellationToken cancellationToken = default);

    Task<GetSignedArtifactUrlResponse> GetSignedArtifactUrlAsync(
        GetSignedArtifactUrlRequest request,
        CancellationToken cancellationToken = default);

    Task<DeleteArtifactResponse> DeleteArtifactAsync(
        DeleteArtifactRequest request,
        CancellationToken cancellationToken = default);
}
