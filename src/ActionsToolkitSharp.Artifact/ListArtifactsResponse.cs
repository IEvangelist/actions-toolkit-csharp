// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// Response from <see cref="IArtifactClient.ListArtifactsAsync"/>. Mirrors
/// upstream <c>ListArtifactsResponse</c>.
/// </summary>
/// <param name="Artifacts">The artifacts returned by the backend.</param>
public sealed record ListArtifactsResponse(IReadOnlyList<Artifact> Artifacts);
