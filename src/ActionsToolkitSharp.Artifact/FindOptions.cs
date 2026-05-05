// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// Identifies a workflow run in another repository so that
/// <see cref="IArtifactClient"/> operations can target artifacts outside the
/// scope of the current run. Mirrors the <c>findBy</c> shape of upstream
/// <c>FindOptions</c> from <c>@actions/artifact</c>.
/// </summary>
/// <param name="Token">A token with <c>actions:read</c> permission on the
/// target repository.</param>
/// <param name="WorkflowRunId">The workflow run identifier of the artifact(s)
/// to look up.</param>
/// <param name="RepositoryOwner">The owner (user or organization) of the
/// target repository.</param>
/// <param name="RepositoryName">The name of the target repository.</param>
public sealed record FindBy(
    string Token,
    long WorkflowRunId,
    string RepositoryOwner,
    string RepositoryName);

/// <summary>
/// Optional cross-workflow lookup parameters. When <see cref="FindBy"/> is
/// supplied, the artifact client routes the operation through the public
/// REST API instead of the in-run Twirp service. Mirrors upstream
/// <c>FindOptions</c>.
/// </summary>
public sealed record FindOptions
{
    /// <summary>
    /// Cross-workflow lookup parameters. When non-null, the operation is
    /// routed through the public REST API.
    /// </summary>
    public FindBy? FindBy { get; init; }
}
