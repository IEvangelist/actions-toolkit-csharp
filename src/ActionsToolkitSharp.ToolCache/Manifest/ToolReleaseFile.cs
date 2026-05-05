// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.ToolCache.Manifest;

/// <summary>
/// Source-gen-friendly DTO for a single file in a versions manifest.
/// </summary>
internal sealed class ToolReleaseFile : IToolReleaseFile
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = "";

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = "";

    [JsonPropertyName("platform_version")]
    public string? PlatformVersion { get; set; }

    [JsonPropertyName("arch")]
    public string Arch { get; set; } = "";

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; } = "";
}
