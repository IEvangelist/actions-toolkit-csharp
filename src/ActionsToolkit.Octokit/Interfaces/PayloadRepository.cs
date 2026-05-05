// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Octokit.Interfaces;

/// <summary>
/// Represents the payload repository.
/// </summary>
public sealed class PayloadRepository
{
    /// <summary>
    /// The full name of the payload repository.
    /// </summary>
    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    /// <summary>
    /// The name of the payload repository.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// The owner of the payload repository.
    /// </summary>
    [JsonPropertyName("owner")]
    public required Owner Owner { get; set; }

    /// <summary>
    /// The URL for the issues HTML of the payload repository.
    /// </summary>
    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }
}
