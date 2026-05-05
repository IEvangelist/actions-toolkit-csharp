// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.ToolCache.Manifest;

/// <summary>
/// Source-gen-friendly DTO for a single release in a versions manifest.
/// </summary>
internal sealed class ToolRelease : IToolRelease
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("stable")]
    public bool Stable { get; set; }

    [JsonPropertyName("release_url")]
    public string ReleaseUrl { get; set; } = "";

    [JsonPropertyName("files")]
    public List<ToolReleaseFile> Files { get; set; } = [];

    IReadOnlyList<IToolReleaseFile> IToolRelease.Files => Files;
}
