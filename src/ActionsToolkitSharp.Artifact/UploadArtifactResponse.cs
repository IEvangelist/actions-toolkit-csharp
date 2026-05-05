// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// The response returned from
/// <see cref="IArtifactClient.UploadArtifactAsync(string, Stream, DateTimeOffset?, CancellationToken)"/>
/// once an artifact has been successfully created, uploaded, and finalized.
/// </summary>
/// <param name="ArtifactId">The backend-assigned numeric identifier of the
/// uploaded artifact.</param>
public sealed record UploadArtifactResponse(long ArtifactId);
