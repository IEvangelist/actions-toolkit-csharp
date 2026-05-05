// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Octokit.Interfaces;

/// <summary>
/// Represents the installation.
/// </summary>
public sealed class Installation
{
    /// <summary>
    /// The unique identifier of the installation.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }
}
