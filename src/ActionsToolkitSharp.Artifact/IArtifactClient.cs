// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// Represents a client for uploading and (in subsequent phases) listing,
/// downloading, and deleting GitHub Actions workflow artifacts. Mirrors the
/// upstream <see href="https://github.com/actions/toolkit/tree/main/packages/artifact">@actions/artifact</see>
/// <c>ArtifactClient</c>.
/// </summary>
public interface IArtifactClient
{
    /// <summary>
    /// Uploads an artifact.
    /// </summary>
    /// <param name="name">The name of the artifact.</param>
    /// <param name="content">A stream containing the (zipped) artifact content
    /// to upload. The official <c>@actions/toolkit</c> and the GitHub UI both
    /// expect this payload to be a zip file.</param>
    /// <param name="expiresAt">Optional expiration timestamp; after this point
    /// the artifact will be eligible for deletion by the backend.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <returns>An <see cref="UploadArtifactResponse"/> containing the
    /// backend-assigned artifact identifier.</returns>
    /// <exception cref="InvalidArtifactNameException"><paramref name="name"/>
    /// is null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="content"/> is
    /// null.</exception>
    /// <exception cref="ArtifactUploadException">The upload failed at the
    /// create, blob upload, or finalize step.</exception>
    /// <exception cref="InvalidArtifactTokenException">The
    /// <c>ACTIONS_RUNTIME_TOKEN</c> environment variable is missing or its JWT
    /// payload does not contain the expected <c>Actions.Results</c> scope
    /// claim.</exception>
    Task<UploadArtifactResponse> UploadArtifactAsync(
        string name,
        Stream content,
        DateTimeOffset? expiresAt = null,
        CancellationToken cancellationToken = default);
}
