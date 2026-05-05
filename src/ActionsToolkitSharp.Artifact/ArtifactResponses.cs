// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

internal sealed class CreateArtifactResponse
{
    [JsonPropertyName("ok")]
    public required bool Ok { get; init; }

    [JsonPropertyName("signed_upload_url")]
    public required string SignedUploadUrl { get; init; }
}

internal sealed class FinalizeArtifactResponse
{
    [JsonPropertyName("ok")]
    public required bool Ok { get; init; }

    [JsonPropertyName("artifact_id")]
    public required long ArtifactId { get; init; }
}

internal sealed class ListArtifactsResponse
{
    [JsonPropertyName("artifacts")]
    public required MonolithArtifact[] Artifacts { get; init; }
}

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

internal sealed class GetSignedArtifactUrlResponse
{
    [JsonPropertyName("signed_url")]
    public required string SignedUrl { get; init; }
}

internal sealed class DeleteArtifactResponse
{
    [JsonPropertyName("ok")]
    public required bool Ok { get; init; }

    [JsonPropertyName("artifact_id")]
    public required long ArtifactId { get; init; }
}
