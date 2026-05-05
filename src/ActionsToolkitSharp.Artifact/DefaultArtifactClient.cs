// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// Default <see cref="IArtifactClient"/> that orchestrates the three-step
/// upload flow against the GitHub Actions results service:
/// <list type="number">
///   <item>Twirp <c>CreateArtifact</c> via <see cref="IArtifactService"/>.</item>
///   <item>HTTP <c>PUT</c> of the supplied stream to the returned signed Azure
///   blob URL via <see cref="IHttpCredentialClientFactory"/> (no auth — the
///   SAS query string carries the credential).</item>
///   <item>Twirp <c>FinalizeArtifact</c> via <see cref="IArtifactService"/>.</item>
/// </list>
/// </summary>
internal sealed class DefaultArtifactClient : IArtifactClient
{
    /// <summary>
    /// Artifact protocol version sent on every <c>CreateArtifact</c> request,
    /// matching upstream <c>@actions/artifact</c> v2.x.
    /// </summary>
    internal const int ArtifactProtocolVersion = 4;

    private const string AzureBlobTypeHeader = "x-ms-blob-type";
    private const string AzureBlobTypeBlockBlob = "BlockBlob";

    private readonly IArtifactService _service;
    private readonly IBackendIdsProvider _backendIdsProvider;
    private readonly IHttpCredentialClientFactory _httpClientFactory;

    public DefaultArtifactClient(
        IArtifactService service,
        IBackendIdsProvider backendIdsProvider,
        IHttpCredentialClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(backendIdsProvider);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _service = service;
        _backendIdsProvider = backendIdsProvider;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<UploadArtifactResponse> UploadArtifactAsync(
        string name,
        Stream content,
        DateTimeOffset? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidArtifactNameException(
                "Artifact name must not be null or empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(content);

        var backendIds = _backendIdsProvider.Get();

        var createRequest = new CreateArtifactRequest
        {
            WorkflowRunBackendId = backendIds.WorkflowRunBackendId,
            WorkflowJobRunBackendId = backendIds.WorkflowJobRunBackendId,
            Name = name,
            Version = ArtifactProtocolVersion,
            ExpiresAt = expiresAt,
        };

        var createResponse = await _service.CreateArtifactAsync(
            createRequest, cancellationToken).ConfigureAwait(false);

        if (!createResponse.Ok)
        {
            throw new ArtifactUploadException(
                "CreateArtifact: response from backend was not ok.");
        }

        await UploadBlobAsync(
            createResponse.SignedUploadUrl, content, cancellationToken).ConfigureAwait(false);

        var finalizeRequest = new FinalizeArtifactRequest
        {
            WorkflowRunBackendId = backendIds.WorkflowRunBackendId,
            WorkflowJobRunBackendId = backendIds.WorkflowJobRunBackendId,
            Name = name,
            Size = content.CanSeek ? content.Length : 0L,
        };

        var finalizeResponse = await _service.FinalizeArtifactAsync(
            finalizeRequest, cancellationToken).ConfigureAwait(false);

        if (!finalizeResponse.Ok)
        {
            throw new ArtifactUploadException(
                "FinalizeArtifact: response from backend was not ok.");
        }

        return new UploadArtifactResponse(finalizeResponse.ArtifactId);
    }

    private async Task UploadBlobAsync(
        string signedUploadUrl,
        Stream content,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(signedUploadUrl))
        {
            throw new ArtifactUploadException(
                "CreateArtifact: backend returned an empty signed upload URL.");
        }

        if (!Uri.TryCreate(signedUploadUrl, UriKind.Absolute, out var uri))
        {
            throw new ArtifactUploadException(
                $"CreateArtifact: backend returned a malformed signed upload URL: '{signedUploadUrl}'.");
        }

        using var blobClient = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Put, uri);
        request.Headers.TryAddWithoutValidation(AzureBlobTypeHeader, AzureBlobTypeBlockBlob);
        request.Content = new StreamContent(content);

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
    }
}
