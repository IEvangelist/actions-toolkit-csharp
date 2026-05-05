// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal.Twirp;

/// <summary>
/// Default <see cref="IArtifactService"/> that issues Twirp-over-HTTP RPCs
/// (POST <c>/twirp/github.actions.results.api.v1.ArtifactService/&lt;Method&gt;</c>)
/// against an <see cref="NetClient"/> whose <c>BaseAddress</c> and bearer
/// authorization header are configured at registration time from
/// <c>ACTIONS_RESULTS_URL</c> / <c>ACTIONS_RUNTIME_TOKEN</c>.
/// </summary>
internal sealed class DefaultArtifactService : IArtifactService
{
    /// <summary>
    /// The Twirp service identifier used to build the request path.
    /// </summary>
    internal const string ArtifactServiceName = "github.actions.results.api.v1.ArtifactService";

    /// <summary>
    /// The named <see cref="IHttpClientFactory"/> client name used by
    /// <c>AddGitHubActionsArtifact</c> on <see cref="IServiceCollection"/>.
    /// </summary>
    internal const string HttpClientName = "ActionsToolkit.Artifact";

    private static ArtifactJsonContext JsonContext => ArtifactJsonContext.Default;

    private readonly NetClient _httpClient;

    public DefaultArtifactService(NetClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    public Task<CreateArtifactResponse> CreateArtifactAsync(
        CreateArtifactRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "CreateArtifact",
            request,
            JsonContext.CreateArtifactRequest,
            JsonContext.CreateArtifactResponse,
            cancellationToken);

    public Task<FinalizeArtifactResponse> FinalizeArtifactAsync(
        FinalizeArtifactRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "FinalizeArtifact",
            request,
            JsonContext.FinalizeArtifactRequest,
            JsonContext.FinalizeArtifactResponse,
            cancellationToken);

    public Task<ListArtifactsResponse> ListArtifactsAsync(
        ListArtifactsRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "ListArtifacts",
            request,
            JsonContext.ListArtifactsRequest,
            JsonContext.ListArtifactsResponse,
            cancellationToken);

    public Task<GetSignedArtifactUrlResponse> GetSignedArtifactUrlAsync(
        GetSignedArtifactUrlRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "GetSignedArtifactURL",
            request,
            JsonContext.GetSignedArtifactUrlRequest,
            JsonContext.GetSignedArtifactUrlResponse,
            cancellationToken);

    public Task<DeleteArtifactResponse> DeleteArtifactAsync(
        DeleteArtifactRequest request,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(
            "DeleteArtifact",
            request,
            JsonContext.DeleteArtifactRequest,
            JsonContext.DeleteArtifactResponse,
            cancellationToken);

    private async Task<TResponse> InvokeAsync<TRequest, TResponse>(
        string method,
        TRequest data,
        JsonTypeInfo<TRequest> requestJsonTypeInfo,
        JsonTypeInfo<TResponse> responseJsonTypeInfo,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(data);

        var requestUri = $"/twirp/{ArtifactServiceName}/{method}";

        using var response = await _httpClient.PostAsJsonAsync(
            requestUri,
            data,
            requestJsonTypeInfo,
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidArtifactResponseException(
                $"{method}: backend returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        var result = await response.Content.ReadFromJsonAsync(
            responseJsonTypeInfo,
            cancellationToken).ConfigureAwait(false);

        return result ?? throw new InvalidArtifactResponseException(
            $"{method}: backend returned an empty response body.");
    }
}
