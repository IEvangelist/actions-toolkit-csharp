// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Response from <see cref="IArtifactClient.DownloadArtifactAsync"/>.
/// Mirrors upstream <c>DownloadArtifactResponse</c>.
/// </summary>
/// <param name="DownloadPath">The directory the artifact was extracted into,
/// or — when <see cref="DownloadArtifactOptions.SkipDecompress"/> is true —
/// the directory the raw payload was written into.</param>
/// <param name="DigestMismatch">When <see cref="DownloadArtifactOptions.ExpectedHash"/>
/// was supplied, true if the downloaded artifact's SHA-256 digest did not
/// match. Null when no expected hash was supplied.</param>
public sealed record DownloadArtifactResponse(
    string DownloadPath,
    bool? DigestMismatch = null);
