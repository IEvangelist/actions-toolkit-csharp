// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal.Twirp;

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

internal sealed class GetSignedArtifactUrlRequest
{
    [JsonPropertyName("workflow_run_backend_id")]
    public required string WorkflowRunBackendId { get; init; }

    [JsonPropertyName("workflow_job_run_backend_id")]
    public required string WorkflowJobRunBackendId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
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
