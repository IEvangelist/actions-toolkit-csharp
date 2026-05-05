// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Manifest;

/// <summary>
/// Source-gen-friendly DTO for the GitHub <c>git/trees</c> API response that
/// <see cref="IToolCacheService.GetManifestFromRepoAsync"/> queries to find
/// the <c>versions-manifest.json</c> blob.
/// </summary>
internal sealed class GitHubTree
{
    [JsonPropertyName("tree")]
    public List<GitHubTreeItem> Tree { get; set; } = [];

    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; }
}

/// <summary>
/// An entry in a GitHub <c>git/trees</c> API response.
/// </summary>
internal sealed class GitHubTreeItem
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("size")]
    [JsonConverter(typeof(LooseStringConverter))]
    public string Size { get; set; } = "";

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";
}

/// <summary>
/// Reads either a JSON string or a JSON number into a <see cref="string"/>.
/// The GitHub trees API has historically returned <c>size</c> as both string
/// and number depending on the route, so be permissive.
/// </summary>
internal sealed class LooseStringConverter : JsonConverter<string>
{
    public override string Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Number when reader.TryGetInt64(out var l) => l.ToString(CultureInfo.InvariantCulture),
            JsonTokenType.Number => reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.Null => string.Empty,
            _ => string.Empty,
        };

    public override void Write(
        Utf8JsonWriter writer,
        string value,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
