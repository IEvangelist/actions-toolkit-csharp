// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Octokit.Interfaces;

/// <summary>
/// The payload of a GitHub webhook.
/// </summary>
public sealed class WebhookPayload
{
    /// <summary>
    /// The payload repository.
    /// </summary>
    [JsonPropertyName("repository")]
    public PayloadRepository? Repository { get; set; }

    /// <summary>
    /// The issue from the webhook payload.
    /// </summary>
    [JsonPropertyName("issue")]
    public WebhookIssue? Issue { get; set; }

    /// <summary>
    /// The pull request from the webhook payload.
    /// </summary>
    [JsonPropertyName("pull_request")]
    public PullRequest? PullRequest { get; set; }

    /// <summary>
    /// The sender of the webhook payload.
    /// </summary>
    [JsonPropertyName("sender")]
    public Sender? Sender { get; set; }

    /// <summary>
    /// The action from the webhook payload.
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    /// <summary>
    /// The installation from the webhook payload.
    /// </summary>
    [JsonPropertyName("installation")]
    public Installation? Installation { get; set; }

    /// <summary>
    /// The comment from the webhook payload.
    /// </summary>
    [JsonPropertyName("comment")]
    public Comment? Comment { get; set; }

    /// <summary>
    /// Gets a JSON <see langword="string"/> representation of the webhook payload.
    /// </summary>
    /// <returns>A JSON <see langword="string"/> representing the webhook payload.</returns>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, SourceGenerationContexts.Default.WebhookPayload);
    }
}
