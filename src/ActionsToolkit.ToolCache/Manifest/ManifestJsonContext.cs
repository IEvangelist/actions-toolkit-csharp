// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Manifest;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for tool-cache
/// manifest types. Native AOT-safe: no reflection-based serialization is
/// used anywhere in <c>ActionsToolkit.ToolCache</c>.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNameCaseInsensitive = true,
    AllowTrailingCommas = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ToolRelease))]
[JsonSerializable(typeof(ToolReleaseFile))]
[JsonSerializable(typeof(List<ToolRelease>))]
[JsonSerializable(typeof(List<ToolReleaseFile>))]
[JsonSerializable(typeof(GitHubTree))]
[JsonSerializable(typeof(GitHubTreeItem))]
internal partial class ManifestJsonContext : JsonSerializerContext;
