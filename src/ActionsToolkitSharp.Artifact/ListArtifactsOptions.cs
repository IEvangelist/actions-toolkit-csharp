// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// Options for <see cref="IArtifactClient.ListArtifactsAsync"/>. Mirrors
/// upstream <c>ListArtifactsOptions &amp; FindOptions</c>.
/// </summary>
public sealed record ListArtifactsOptions
{
    /// <summary>
    /// When true, filters duplicate artifact names so that only the most
    /// recent (highest <c>id</c>) is returned. Useful in workflow re-runs.
    /// </summary>
    public bool Latest { get; init; }

    /// <summary>
    /// Optional cross-workflow lookup parameters. When non-null, the
    /// operation routes through the public REST API.
    /// </summary>
    public FindBy? FindBy { get; init; }
}
