// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal.Twirp;

/// <summary>
/// Internal abstraction over the Twirp-over-HTTP RPCs exposed by the GitHub
/// Actions results service (<c>github.actions.results.api.v1.ArtifactService</c>).
/// All five methods correspond directly to upstream operations.
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
