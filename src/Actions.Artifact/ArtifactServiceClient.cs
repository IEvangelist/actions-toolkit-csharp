using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Actions.Artifact;

internal sealed class ArtifactServiceClient : IDisposable
{
    private const string ArtifactServiceName = "github.actions.results.api.v1.ArtifactService";

    private static ArtifactServiceClientContext JsonContext => ArtifactServiceClientContext.Default;

    private readonly HttpClient _httpClient;

    public ArtifactServiceClient(string runtimeToken, Uri resultsServiceUrl)
    {
        ArgumentNullException.ThrowIfNull(runtimeToken);
        ArgumentNullException.ThrowIfNull(resultsServiceUrl);

        _httpClient = new HttpClient
        {
            DefaultRequestHeaders =
            {
                UserAgent = { new ProductInfoHeaderValue("GitHub.Actions.Artifact", null) },
                Authorization = new AuthenticationHeaderValue("Bearer", runtimeToken),
            },
            BaseAddress = resultsServiceUrl,
        };
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private async Task<TResponse> RequestAsync<TRequest, TResponse>(
        string method,
        TRequest data,
        JsonTypeInfo<TRequest> requestJsonTypeInfo,
        JsonTypeInfo<TResponse> responseJsonTypeInfo,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync($"/twirp/{ArtifactServiceName}/{method}", data, requestJsonTypeInfo, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync(responseJsonTypeInfo, cancellationToken) ?? throw new InvalidOperationException("Response was empty");
    }

    public async Task<CreateArtifactResponse> CreateArtifactAsync(CreateArtifactRequest data, CancellationToken cancellationToken = default)
    {
        return await RequestAsync("CreateArtifact", data, JsonContext.CreateArtifactRequest, JsonContext.CreateArtifactResponse, cancellationToken);
    }

    public async Task<FinalizeArtifactResponse> FinalizeArtifactAsync(FinalizeArtifactRequest data, CancellationToken cancellationToken = default)
    {
        return await RequestAsync("FinalizeArtifact", data, JsonContext.FinalizeArtifactRequest, JsonContext.FinalizeArtifactResponse, cancellationToken);
    }

    public async Task<ListArtifactsResponse> ListArtifactsAsync(ListArtifactsRequest data, CancellationToken cancellationToken = default)
    {
        return await RequestAsync("ListArtifacts", data, JsonContext.ListArtifactsRequest, JsonContext.ListArtifactsResponse, cancellationToken);
    }

    public async Task<GetSignedArtifactUrlResponse> GetSignedArtifactUrlAsync(GetSignedArtifactUrlRequest data, CancellationToken cancellationToken = default)
    {
        return await RequestAsync("GetSignedArtifactURL", data, JsonContext.GetSignedArtifactUrlRequest, JsonContext.GetSignedArtifactUrlResponse, cancellationToken);
    }

    public async Task<DeleteArtifactResponse> DeleteArtifactAsync(DeleteArtifactRequest data, CancellationToken cancellationToken = default)
    {
        return await RequestAsync("DeleteArtifact", data, JsonContext.DeleteArtifactRequest, JsonContext.DeleteArtifactResponse, cancellationToken);
    }
}

[JsonSerializable(typeof(CreateArtifactRequest))]
[JsonSerializable(typeof(CreateArtifactResponse))]
[JsonSerializable(typeof(FinalizeArtifactRequest))]
[JsonSerializable(typeof(FinalizeArtifactResponse))]
[JsonSerializable(typeof(ListArtifactsRequest))]
[JsonSerializable(typeof(ListArtifactsResponse))]
[JsonSerializable(typeof(ListArtifactsResponse.MonolithArtifact))]
[JsonSerializable(typeof(GetSignedArtifactUrlRequest))]
[JsonSerializable(typeof(GetSignedArtifactUrlResponse))]
[JsonSerializable(typeof(DeleteArtifactRequest))]
[JsonSerializable(typeof(DeleteArtifactResponse))]
[JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.AllowReadingFromString)]
internal partial class ArtifactServiceClientContext : JsonSerializerContext;

internal sealed class CreateArtifactRequest
{
    [JsonPropertyName("workflow_run_backend_id")]
    public required string WorkflowRunBackendId { get; init; }

    [JsonPropertyName("workflow_job_run_backend_id")]
    public required string WorkflowJobRunBackendId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; init; }

    [JsonPropertyName("version")]
    public required int Version { get; init; }
}

internal sealed class CreateArtifactResponse
{
    [JsonPropertyName("ok")]
    public required bool Ok { get; init; }

    [JsonPropertyName("signed_upload_url")]
    public required string SignedUploadUrl { get; init; }
}

internal sealed class FinalizeArtifactRequest
{
    [JsonPropertyName("workflow_run_backend_id")]
    public required string WorkflowRunBackendId { get; init; }

    [JsonPropertyName("workflow_job_run_backend_id")]
    public required string WorkflowJobRunBackendId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("size")]
    public required long Size { get; init; }

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }
}

internal sealed class FinalizeArtifactResponse
{
    [JsonPropertyName("ok")]
    public required bool Ok { get; init; }

    [JsonPropertyName("artifact_id")]
    public required long ArtifactId { get; init; }
}

internal sealed class ListArtifactsRequest
{
    [JsonPropertyName("workflow_run_backend_id")]
    public required string WorkflowRunBackendId { get; init; }

    [JsonPropertyName("workflow_job_run_backend_id")]
    public required string WorkflowJobRunBackendId { get; init; }

    [JsonPropertyName("name_filter")]
    public string? NameFilter { get; init; }

    [JsonPropertyName("id_filter")]
    public long? IdFilter { get; init; }
}

internal sealed class ListArtifactsResponse
{
    [JsonPropertyName("artifacts")]
    public required MonolithArtifact[] Artifacts { get; init; }

    internal sealed class MonolithArtifact
    {
        [JsonPropertyName("workflow_run_backend_id")]
        public required string WorkflowRunBackendId { get; init; }

        [JsonPropertyName("workflow_job_run_backend_id")]
        public required string WorkflowJobRunBackendId { get; init; }

        [JsonPropertyName("database_id")]
        public required long DatabaseId { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("size")]
        public required long Size { get; init; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset? CreatedAt { get; init; }
    }
}

internal sealed class GetSignedArtifactUrlRequest
{
    [JsonPropertyName("workflow_run_backend_id")]
    public required string WorkflowRunBackendId { get; init; }

    [JsonPropertyName("workflow_job_run_backend_id")]
    public required string WorkflowJobRunBackendId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

internal sealed class GetSignedArtifactUrlResponse
{
    [JsonPropertyName("signed_url")]
    public required string SignedUrl { get; init; }
}

internal sealed class DeleteArtifactRequest
{
    [JsonPropertyName("workflow_run_backend_id")]
    public required string WorkflowRunBackendId { get; init; }

    [JsonPropertyName("workflow_job_run_backend_id")]
    public required string WorkflowJobRunBackendId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

internal sealed class DeleteArtifactResponse
{
    [JsonPropertyName("ok")]
    public required bool Ok { get; init; }

    [JsonPropertyName("artifact_id")]
    public required long ArtifactId { get; init; }
}
