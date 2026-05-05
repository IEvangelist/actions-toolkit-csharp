// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Options for <see cref="IArtifactClient.GetArtifactAsync"/>. Mirrors
/// upstream <c>FindOptions</c>.
/// </summary>
public sealed record GetArtifactOptions
{
    /// <summary>
    /// Optional cross-workflow lookup parameters. When non-null, the
    /// operation routes through the public REST API.
    /// </summary>
    public FindBy? FindBy { get; init; }
}
