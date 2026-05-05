// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Options for <see cref="IArtifactClient.DownloadArtifactAsync"/>. Mirrors
/// upstream <c>DownloadArtifactOptions &amp; FindOptions</c>.
/// </summary>
public sealed record DownloadArtifactOptions
{
    /// <summary>
    /// Filesystem directory the artifact zip should be extracted into. When
    /// null, the directory is the value of the <c>GITHUB_WORKSPACE</c>
    /// environment variable, or — if that is not set —
    /// <see cref="Environment.CurrentDirectory"/>.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Optional expected SHA-256 digest computed at upload time. When
    /// supplied the download will set
    /// <see cref="DownloadArtifactResponse.DigestMismatch"/> to true if the
    /// downloaded artifact's digest does not match.
    /// </summary>
    public string? ExpectedHash { get; init; }

    /// <summary>
    /// When true, skip the zip extraction step and write the raw payload to
    /// disk under <see cref="Path"/>. Mirrors upstream
    /// <c>skipDecompress</c>.
    /// </summary>
    public bool SkipDecompress { get; init; }

    /// <summary>
    /// Optional cross-workflow lookup parameters. When non-null, the
    /// operation routes through the public REST API.
    /// </summary>
    public FindBy? FindBy { get; init; }
}
