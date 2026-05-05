// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography;

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// Default <see cref="IArtifactClient"/> that orchestrates the artifact
/// pipeline against the GitHub Actions results service. Mirrors the upstream
/// <c>@actions/artifact</c> <c>DefaultArtifactClient</c>.
/// </summary>
internal sealed class DefaultArtifactClient : IArtifactClient
{
    /// <summary>
    /// Artifact protocol version sent on every <c>CreateArtifact</c> request.
    /// Mirrors upstream <c>@actions/artifact</c> v2.x (<c>version: 7</c> in
    /// the latest source).
    /// </summary>
    internal const int ArtifactProtocolVersion = 4;

    private const string AzureBlobTypeHeader = "x-ms-blob-type";
    private const string AzureBlobTypeBlockBlob = "BlockBlob";
    private const string ZipMimeType = "application/zip";

    private readonly IArtifactService _service;
    private readonly IBackendIdsProvider _backendIdsProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPublicArtifactsApi _publicApi;

    /// <summary>
    /// The named <see cref="IHttpClientFactory"/> client used for blob
    /// upload/download against the signed URLs returned by the artifact
    /// service. No authorization header — the URL itself is presigned.
    /// </summary>
    internal const string BlobHttpClientName = "ActionsToolkitSharp.Artifact.Blob";

    public DefaultArtifactClient(
        IArtifactService service,
        IBackendIdsProvider backendIdsProvider,
        IHttpClientFactory httpClientFactory,
        IPublicArtifactsApi publicApi)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(backendIdsProvider);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(publicApi);

        _service = service;
        _backendIdsProvider = backendIdsProvider;
        _httpClientFactory = httpClientFactory;
        _publicApi = publicApi;
    }

    /// <inheritdoc />
    public async ValueTask<UploadArtifactResponse> UploadArtifactAsync(
        string name,
        IEnumerable<string> files,
        string rootDirectory,
        UploadArtifactOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentException.ThrowIfNullOrEmpty(rootDirectory);

        if (ArtifactConfig.IsGhes())
        {
            throw new GhesNotSupportedException();
        }

        PathAndArtifactNameValidation.ValidateArtifactName(name);
        UploadZipSpecification.ValidateRootDirectory(rootDirectory);

        var spec = UploadZipSpecification.GetUploadZipSpecification(files, rootDirectory);
        if (spec.Count == 0)
        {
            throw new FilesNotFoundException(files as IReadOnlyList<string> ?? files.ToArray());
        }

        var backendIds = _backendIdsProvider.Get();

        var createRequest = new CreateArtifactRequest
        {
            WorkflowRunBackendId = backendIds.WorkflowRunBackendId,
            WorkflowJobRunBackendId = backendIds.WorkflowJobRunBackendId,
            Name = name,
            Version = ArtifactProtocolVersion,
            ExpiresAt = ArtifactRetention.GetExpiration(options?.RetentionDays),
        };

        var createResponse = await _service.CreateArtifactAsync(
            createRequest, cancellationToken).ConfigureAwait(false);

        if (!createResponse.Ok)
        {
            throw new InvalidArtifactResponseException(
                "CreateArtifact: response from backend was not ok.");
        }

        var zipResult = await ZipUploadStream.CreateAsync(
            spec, options?.CompressionLevel, cancellationToken).ConfigureAwait(false);

        string digest;
        try
        {
            digest = await UploadBlobAsync(
                createResponse.SignedUploadUrl, zipResult.Content, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await zipResult.Content.DisposeAsync().ConfigureAwait(false);
        }

        var finalizeRequest = new FinalizeArtifactRequest
        {
            WorkflowRunBackendId = backendIds.WorkflowRunBackendId,
            WorkflowJobRunBackendId = backendIds.WorkflowJobRunBackendId,
            Name = name,
            Size = zipResult.UploadSize,
            Hash = $"sha256:{digest}",
        };

        var finalizeResponse = await _service.FinalizeArtifactAsync(
            finalizeRequest, cancellationToken).ConfigureAwait(false);

        if (!finalizeResponse.Ok)
        {
            throw new InvalidArtifactResponseException(
                "FinalizeArtifact: response from backend was not ok.");
        }

        return new UploadArtifactResponse(finalizeResponse.ArtifactId, zipResult.UploadSize, digest);
    }

    /// <inheritdoc />
    public async ValueTask<ListArtifactsResponse> ListArtifactsAsync(
        ListArtifactsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (ArtifactConfig.IsGhes())
        {
            throw new GhesNotSupportedException();
        }

        IReadOnlyList<Artifact> artifacts;
        if (options?.FindBy is { } findBy)
        {
            artifacts = await _publicApi.ListAsync(findBy, nameFilter: null, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var backendIds = _backendIdsProvider.Get();
            var listResponse = await _service.ListArtifactsAsync(
                new ListArtifactsRequest
                {
                    WorkflowRunBackendId = backendIds.WorkflowRunBackendId,
                    WorkflowJobRunBackendId = backendIds.WorkflowJobRunBackendId,
                },
                cancellationToken).ConfigureAwait(false);

            var mapped = new Artifact[listResponse.Artifacts.Length];
            for (var i = 0; i < listResponse.Artifacts.Length; i++)
            {
                var src = listResponse.Artifacts[i];
                mapped[i] = new Artifact(src.Name, src.DatabaseId, src.Size, src.CreatedAt);
            }

            artifacts = mapped;
        }

        if (options?.Latest == true)
        {
            artifacts = FilterLatest(artifacts);
        }

        return new ListArtifactsResponse(artifacts);
    }

    /// <inheritdoc />
    public async ValueTask<GetArtifactResponse> GetArtifactAsync(
        string artifactName,
        GetArtifactOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(artifactName);

        if (ArtifactConfig.IsGhes())
        {
            throw new GhesNotSupportedException();
        }

        IReadOnlyList<Artifact> artifacts;
        if (options?.FindBy is { } findBy)
        {
            artifacts = await _publicApi.ListAsync(findBy, artifactName, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var backendIds = _backendIdsProvider.Get();
            var listResponse = await _service.ListArtifactsAsync(
                new ListArtifactsRequest
                {
                    WorkflowRunBackendId = backendIds.WorkflowRunBackendId,
                    WorkflowJobRunBackendId = backendIds.WorkflowJobRunBackendId,
                    NameFilter = artifactName,
                },
                cancellationToken).ConfigureAwait(false);

            var mapped = new Artifact[listResponse.Artifacts.Length];
            for (var i = 0; i < listResponse.Artifacts.Length; i++)
            {
                var src = listResponse.Artifacts[i];
                mapped[i] = new Artifact(src.Name, src.DatabaseId, src.Size, src.CreatedAt);
            }

            artifacts = mapped;
        }

        if (artifacts.Count == 0)
        {
            throw new ArtifactNotFoundException(
                $"Artifact not found for name: {artifactName}");
        }

        // When more than one artifact shares a name (legacy uploads or
        // re-runs), upstream returns the highest-id (newest) entry.
        var newest = artifacts[0];
        for (var i = 1; i < artifacts.Count; i++)
        {
            if (artifacts[i].Id > newest.Id)
            {
                newest = artifacts[i];
            }
        }

        return new GetArtifactResponse(newest);
    }

    /// <inheritdoc />
    public async ValueTask<DownloadArtifactResponse> DownloadArtifactAsync(
        long artifactId,
        DownloadArtifactOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (ArtifactConfig.IsGhes())
        {
            throw new GhesNotSupportedException();
        }

        var downloadDir = ResolveOrCreateDirectory(options?.Path);

        Uri downloadUri;
        if (options?.FindBy is { } findBy)
        {
            downloadUri = await _publicApi.GetDownloadRedirectAsync(
                findBy, artifactId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var backendIds = _backendIdsProvider.Get();
            var listResponse = await _service.ListArtifactsAsync(
                new ListArtifactsRequest
                {
                    WorkflowRunBackendId = backendIds.WorkflowRunBackendId,
                    WorkflowJobRunBackendId = backendIds.WorkflowJobRunBackendId,
                    IdFilter = artifactId,
                },
                cancellationToken).ConfigureAwait(false);

            if (listResponse.Artifacts.Length == 0)
            {
                throw new ArtifactNotFoundException(
                    $"No artifacts found for ID: {artifactId}");
            }

            var artifact = listResponse.Artifacts[0];
            var signedResponse = await _service.GetSignedArtifactUrlAsync(
                new GetSignedArtifactUrlRequest
                {
                    WorkflowRunBackendId = artifact.WorkflowRunBackendId,
                    WorkflowJobRunBackendId = artifact.WorkflowJobRunBackendId,
                    Name = artifact.Name,
                },
                cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(signedResponse.SignedUrl)
                || !Uri.TryCreate(signedResponse.SignedUrl, UriKind.Absolute, out var parsed))
            {
                throw new InvalidArtifactResponseException(
                    "GetSignedArtifactURL: backend returned an empty or malformed signed URL.");
            }

            downloadUri = parsed;
        }

        var digest = await DownloadAndExtractAsync(
            downloadUri, downloadDir, options?.SkipDecompress ?? false, cancellationToken).ConfigureAwait(false);

        bool? digestMismatch = null;
        if (!string.IsNullOrEmpty(options?.ExpectedHash))
        {
            digestMismatch = !string.Equals(options.ExpectedHash, digest, StringComparison.Ordinal);
        }

        return new DownloadArtifactResponse(downloadDir, digestMismatch);
    }

    /// <inheritdoc />
    public async ValueTask<DeleteArtifactResponse> DeleteArtifactAsync(
        string artifactName,
        DeleteArtifactOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(artifactName);

        if (ArtifactConfig.IsGhes())
        {
            throw new GhesNotSupportedException();
        }

        if (options?.FindBy is { } findBy)
        {
            var existing = await _publicApi.ListAsync(findBy, artifactName, cancellationToken).ConfigureAwait(false);
            if (existing.Count == 0)
            {
                throw new ArtifactNotFoundException(
                    $"Artifact not found for name: {artifactName}");
            }

            var newestId = existing[0].Id;
            for (var i = 1; i < existing.Count; i++)
            {
                if (existing[i].Id > newestId)
                {
                    newestId = existing[i].Id;
                }
            }

            await _publicApi.DeleteAsync(findBy, newestId, cancellationToken).ConfigureAwait(false);
            return new DeleteArtifactResponse(newestId);
        }
        else
        {
            var backendIds = _backendIdsProvider.Get();
            var listResponse = await _service.ListArtifactsAsync(
                new ListArtifactsRequest
                {
                    WorkflowRunBackendId = backendIds.WorkflowRunBackendId,
                    WorkflowJobRunBackendId = backendIds.WorkflowJobRunBackendId,
                    NameFilter = artifactName,
                },
                cancellationToken).ConfigureAwait(false);

            if (listResponse.Artifacts.Length == 0)
            {
                throw new ArtifactNotFoundException(
                    $"Artifact not found for name: {artifactName}");
            }

            var artifact = listResponse.Artifacts[0];
            for (var i = 1; i < listResponse.Artifacts.Length; i++)
            {
                if (listResponse.Artifacts[i].DatabaseId > artifact.DatabaseId)
                {
                    artifact = listResponse.Artifacts[i];
                }
            }

            var deleteResponse = await _service.DeleteArtifactAsync(
                new DeleteArtifactRequest
                {
                    WorkflowRunBackendId = artifact.WorkflowRunBackendId,
                    WorkflowJobRunBackendId = artifact.WorkflowJobRunBackendId,
                    Name = artifact.Name,
                },
                cancellationToken).ConfigureAwait(false);

            if (!deleteResponse.Ok)
            {
                throw new InvalidArtifactResponseException(
                    "DeleteArtifact: response from backend was not ok.");
            }

            return new DeleteArtifactResponse(deleteResponse.ArtifactId);
        }
    }

    private async Task<string> UploadBlobAsync(
        string signedUploadUrl,
        Stream content,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(signedUploadUrl))
        {
            throw new InvalidArtifactResponseException(
                "CreateArtifact: backend returned an empty signed upload URL.");
        }

        if (!Uri.TryCreate(signedUploadUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidArtifactResponseException(
                $"CreateArtifact: backend returned a malformed signed upload URL: '{signedUploadUrl}'.");
        }

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        var digest = await ComputeDigestAsync(content, cancellationToken).ConfigureAwait(false);
        if (content.CanSeek)
        {
            content.Position = 0;
        }

        using var blobClient = _httpClientFactory.CreateClient(BlobHttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Put, uri);
        request.Headers.TryAddWithoutValidation(AzureBlobTypeHeader, AzureBlobTypeBlockBlob);
        request.Content = new StreamContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(ZipMimeType);

        HttpResponseMessage response;
        try
        {
            response = await blobClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new ArtifactUploadException(
                "Blob upload failed: the HTTP request to the signed upload URL did not complete.", ex);
        }

        try
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new ArtifactUploadException(
                    $"Blob upload failed: backend returned {(int)response.StatusCode} {response.ReasonPhrase}.");
            }
        }
        finally
        {
            response.Dispose();
        }

        return digest;
    }

    private static async Task<string> ComputeDigestAsync(Stream content, CancellationToken cancellationToken)
    {
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(content, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexStringLower(hash);
    }

    private async Task<string?> DownloadAndExtractAsync(
        Uri downloadUri,
        string targetDirectory,
        bool skipDecompress,
        CancellationToken cancellationToken)
    {
        using var blobClient = _httpClientFactory.CreateClient(BlobHttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUri);

        HttpResponseMessage response;
        try
        {
            response = await blobClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new ArtifactUploadException(
                "Artifact download failed: the HTTP request to the signed URL did not complete.", ex);
        }

        try
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidArtifactResponseException(
                    $"Artifact download failed: backend returned {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            await using var payloadStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            var stagingPath = Path.Combine(Path.GetTempPath(), $"ats-artifact-dl-{Guid.NewGuid():N}.zip");
            string digest;
            try
            {
                await using (var staging = new FileStream(
                    stagingPath,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true))
                {
                    await payloadStream.CopyToAsync(staging, cancellationToken).ConfigureAwait(false);
                    staging.Position = 0;
                    digest = $"sha256:{await ComputeDigestAsync(staging, cancellationToken).ConfigureAwait(false)}";
                    staging.Position = 0;

                    if (skipDecompress)
                    {
                        var fileName = ExtractFileName(downloadUri, response.Content.Headers);
                        var destinationPath = Path.Combine(targetDirectory, fileName);
                        await using var outFile = new FileStream(
                            destinationPath, FileMode.Create, FileAccess.Write, FileShare.None,
                            bufferSize: 81920, useAsync: true);
                        await staging.CopyToAsync(outFile, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        ExtractZip(staging, targetDirectory);
                    }
                }
            }
            finally
            {
                if (File.Exists(stagingPath))
                {
                    try { File.Delete(stagingPath); } catch { /* best effort */ }
                }
            }

            return digest;
        }
        finally
        {
            response.Dispose();
        }
    }

    private static void ExtractZip(Stream zipStream, string targetDirectory)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
        var fullTargetDir = Path.GetFullPath(targetDirectory);
        foreach (var entry in archive.Entries)
        {
            var destination = Path.GetFullPath(Path.Combine(targetDirectory, entry.FullName));
            if (!destination.StartsWith(fullTargetDir, StringComparison.Ordinal))
            {
                throw new InvalidArtifactResponseException(
                    $"Refusing to extract entry '{entry.FullName}' outside the target directory.");
            }

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destination);
                continue;
            }

            var dir = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            entry.ExtractToFile(destination, overwrite: true);
        }
    }

    private static string ExtractFileName(Uri uri, HttpContentHeaders contentHeaders)
    {
        var disposition = contentHeaders.ContentDisposition;
        if (!string.IsNullOrEmpty(disposition?.FileNameStar))
        {
            return Path.GetFileName(disposition!.FileNameStar);
        }

        if (!string.IsNullOrEmpty(disposition?.FileName))
        {
            return Path.GetFileName(disposition!.FileName.Trim('"'));
        }

        var path = uri.AbsolutePath;
        var name = Path.GetFileName(path);
        return string.IsNullOrEmpty(name) ? "artifact" : name;
    }

    private static string ResolveOrCreateDirectory(string? requestedPath)
    {
        var directory = string.IsNullOrEmpty(requestedPath)
            ? ArtifactConfig.GetGitHubWorkspaceDirectory()
            : requestedPath;

        Directory.CreateDirectory(directory);
        return directory;
    }

    private static List<Artifact> FilterLatest(IReadOnlyList<Artifact> artifacts)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var ordered = artifacts.OrderByDescending(static a => a.Id).ToArray();
        var result = new List<Artifact>(artifacts.Count);
        foreach (var artifact in ordered)
        {
            if (seen.Add(artifact.Name))
            {
                result.Add(artifact);
            }
        }

        return result;
    }
}
