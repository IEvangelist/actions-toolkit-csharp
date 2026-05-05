// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// Client for uploading, listing, getting, downloading, and deleting GitHub
/// Actions workflow artifacts. Mirrors the upstream
/// <see href="https://github.com/actions/toolkit/tree/main/packages/artifact">@actions/artifact</see>
/// <c>ArtifactClient</c> surface.
/// </summary>
public interface IArtifactClient
{
    /// <summary>
    /// Builds a zip from <paramref name="files"/> (relative to
    /// <paramref name="rootDirectory"/>) and uploads it as a workflow
    /// artifact named <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the artifact.</param>
    /// <param name="files">A list of absolute or relative paths to include in
    /// the zip. Each path must live under <paramref name="rootDirectory"/>.</param>
    /// <param name="rootDirectory">The directory whose contents are being
    /// uploaded; the destination paths inside the zip are computed relative
    /// to this directory.</param>
    /// <param name="options">Optional upload-time options
    /// (<see cref="UploadArtifactOptions.RetentionDays"/>,
    /// <see cref="UploadArtifactOptions.CompressionLevel"/>).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <exception cref="GhesNotSupportedException">Running on a GHES
    /// instance.</exception>
    /// <exception cref="InvalidArtifactNameException"><paramref name="name"/>
    /// is null, empty, or contains an illegal character.</exception>
    /// <exception cref="FilesNotFoundException">No files matched.</exception>
    /// <exception cref="InvalidArtifactResponseException">A Twirp request
    /// returned <c>ok=false</c> or otherwise unexpected.</exception>
    /// <exception cref="ArtifactUploadException">The blob PUT failed.</exception>
    /// <exception cref="InvalidArtifactTokenException">The
    /// <c>ACTIONS_RUNTIME_TOKEN</c> is missing or unparseable.</exception>
    ValueTask<UploadArtifactResponse> UploadArtifactAsync(
        string name,
        IEnumerable<string> files,
        string rootDirectory,
        UploadArtifactOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists artifacts produced by the current workflow run. When
    /// <see cref="ListArtifactsOptions.FindBy"/> is supplied, lists artifacts
    /// from another run via the public REST API instead.
    /// </summary>
    /// <param name="options">Optional list-time options
    /// (<see cref="ListArtifactsOptions.Latest"/>,
    /// <see cref="ListArtifactsOptions.FindBy"/>).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <exception cref="GhesNotSupportedException">Running on a GHES
    /// instance.</exception>
    /// <exception cref="InvalidArtifactResponseException">The Twirp service
    /// or REST API returned an unexpected response.</exception>
    /// <exception cref="InvalidArtifactTokenException">The
    /// <c>ACTIONS_RUNTIME_TOKEN</c> is missing or unparseable (in-run mode
    /// only).</exception>
    ValueTask<ListArtifactsResponse> ListArtifactsAsync(
        ListArtifactsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an artifact by <paramref name="artifactName"/> in the current
    /// workflow run, or — when <see cref="GetArtifactOptions.FindBy"/> is
    /// supplied — in the referenced run via the public REST API.
    /// </summary>
    /// <param name="artifactName">The name of the artifact to find.</param>
    /// <param name="options">Optional get-time options.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <exception cref="GhesNotSupportedException">Running on a GHES
    /// instance.</exception>
    /// <exception cref="ArtifactNotFoundException">No matching artifact
    /// exists.</exception>
    /// <exception cref="InvalidArtifactResponseException">An unexpected
    /// response was returned.</exception>
    /// <exception cref="InvalidArtifactTokenException">The
    /// <c>ACTIONS_RUNTIME_TOKEN</c> is missing or unparseable (in-run mode
    /// only).</exception>
    ValueTask<GetArtifactResponse> GetArtifactAsync(
        string artifactName,
        GetArtifactOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the artifact identified by <paramref name="artifactId"/>
    /// (in the current run, or — when <see cref="DownloadArtifactOptions.FindBy"/>
    /// is supplied — via the public REST API). The artifact is extracted
    /// into <see cref="DownloadArtifactOptions.Path"/> unless
    /// <see cref="DownloadArtifactOptions.SkipDecompress"/> is true.
    /// </summary>
    /// <param name="artifactId">The backend identifier of the artifact to
    /// download.</param>
    /// <param name="options">Optional download-time options.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <exception cref="GhesNotSupportedException">Running on a GHES
    /// instance.</exception>
    /// <exception cref="ArtifactNotFoundException">No matching artifact
    /// exists.</exception>
    /// <exception cref="InvalidArtifactResponseException">An unexpected
    /// response was returned.</exception>
    /// <exception cref="InvalidArtifactTokenException">The
    /// <c>ACTIONS_RUNTIME_TOKEN</c> is missing or unparseable (in-run mode
    /// only).</exception>
    ValueTask<DownloadArtifactResponse> DownloadArtifactAsync(
        long artifactId,
        DownloadArtifactOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an artifact by <paramref name="artifactName"/> from the
    /// current run, or — when <see cref="DeleteArtifactOptions.FindBy"/> is
    /// supplied — from the referenced run via the public REST API.
    /// </summary>
    /// <param name="artifactName">The name of the artifact to delete.</param>
    /// <param name="options">Optional delete-time options.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <exception cref="GhesNotSupportedException">Running on a GHES
    /// instance.</exception>
    /// <exception cref="ArtifactNotFoundException">No matching artifact
    /// exists.</exception>
    /// <exception cref="InvalidArtifactResponseException">An unexpected
    /// response was returned.</exception>
    /// <exception cref="InvalidArtifactTokenException">The
    /// <c>ACTIONS_RUNTIME_TOKEN</c> is missing or unparseable (in-run mode
    /// only).</exception>
    ValueTask<DeleteArtifactResponse> DeleteArtifactAsync(
        string artifactName,
        DeleteArtifactOptions? options = null,
        CancellationToken cancellationToken = default);
}
