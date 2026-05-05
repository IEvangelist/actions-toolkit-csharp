// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Response from <see cref="IArtifactClient.DeleteArtifactAsync"/>. Mirrors
/// upstream <c>DeleteArtifactResponse</c>.
/// </summary>
/// <param name="Id">The backend identifier of the deleted artifact.</param>
public sealed record DeleteArtifactResponse(long Id);
