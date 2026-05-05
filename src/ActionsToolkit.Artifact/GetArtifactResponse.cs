// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Response from <see cref="IArtifactClient.GetArtifactAsync"/>. Mirrors
/// upstream <c>GetArtifactResponse</c>.
/// </summary>
/// <param name="Artifact">The matching artifact.</param>
public sealed record GetArtifactResponse(Artifact Artifact);
