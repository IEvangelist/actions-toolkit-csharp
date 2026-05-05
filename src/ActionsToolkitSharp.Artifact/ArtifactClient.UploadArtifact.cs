using Azure.Storage.Blobs;

namespace Actions.Artifact;

public sealed partial class ArtifactClient
{
    public sealed record UploadArtifactResponse(long ArtifactId);

    /// <summary>
    /// Uploads an artifact.
    /// </summary>
    /// <param name="name">The name of the artifact, required.</param>
    /// <param name="content">A stream containing the content to upload.</param>
    /// <param name="expiresAt">Date after which the artifact will expire.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <remarks>The official actions/toolkit and GitHub UI expect the <paramref name="content"/> to be a zip file.</remarks>
    public async Task<UploadArtifactResponse> UploadArtifactAsync(string name, Stream content, DateTimeOffset? expiresAt = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(content);

        var createArtifactRequest = new CreateArtifactRequest
        {
            WorkflowRunBackendId = _backendIds.WorkflowRunBackendId,
            WorkflowJobRunBackendId = _backendIds.WorkflowJobRunBackendId,
            Name = name,
            Version = 4,
            ExpiresAt = expiresAt,
        };

        var createArtifactResponse = await _client.CreateArtifactAsync(createArtifactRequest, cancellationToken);
        if (!createArtifactResponse.Ok)
        {
            throw new Exception("CreateArtifact: response from backend was not ok");
        }

        var blobClient = new BlobClient(new Uri(createArtifactResponse.SignedUploadUrl));
        await blobClient.UploadAsync(content, cancellationToken);

        var finalizeArtifactRequest = new FinalizeArtifactRequest
        {
            WorkflowRunBackendId = _backendIds.WorkflowRunBackendId,
            WorkflowJobRunBackendId = _backendIds.WorkflowJobRunBackendId,
            Name = name,
            Size = content.Length,
        };

        var finalizeArtifactResponse = await _client.FinalizeArtifactAsync(finalizeArtifactRequest, cancellationToken);
        if (!finalizeArtifactResponse.Ok)
        {
            throw new Exception("FinalizeArtifact: response from backend was not ok");
        }

        return new UploadArtifactResponse(finalizeArtifactResponse.ArtifactId);
    }
}
