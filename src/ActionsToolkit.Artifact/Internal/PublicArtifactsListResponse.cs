// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// Wire shape for the GitHub REST API
/// <c>GET /repos/{owner}/{repo}/actions/runs/{run_id}/artifacts</c> response.
/// </summary>
internal sealed class PublicArtifactsListResponse
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; init; }

    [JsonPropertyName("artifacts")]
    public PublicArtifactItem[] Artifacts { get; init; } = [];
}

/// <summary>
/// Wire shape for a single artifact entry in the GitHub REST API artifacts
/// list response.
/// </summary>
internal sealed class PublicArtifactItem
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("size_in_bytes")]
    public long SizeInBytes { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("digest")]
    public string? Digest { get; init; }
}
