// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Issue = ActionsToolkit.Octokit.Common.Issue;
using Repository = ActionsToolkit.Octokit.Common.Repository;

namespace ActionsToolkit.Octokit;

/// <summary>
/// Provides access to the GitHub Actions context.
/// </summary>
public sealed class Context
{
    private static readonly Lazy<Context> s_context = new(() => new());

    /// <summary>
    /// Gets the current GitHub Actions context.
    /// </summary>
    public static Context Current => s_context.Value;

    /// <summary>
    /// Gets webhook payload in context.
    /// </summary>
    public WebhookPayload? Payload { get; }

    /// <summary>
    /// Gets event name in context.
    /// </summary>
    public string? EventName { get; }

    /// <summary>
    /// Gets the SHA in context.
    /// </summary>
    public string? Sha { get; }

    /// <summary>
    /// Gets the REF in context.
    /// </summary>
    public string? Ref { get; }

    /// <summary>
    /// Gets the workflow in context.
    /// </summary>
    public string? Workflow { get; }

    /// <summary>
    /// Gets the action in context.
    /// </summary>
    public string? Action { get; }

    /// <summary>
    /// Gets the actor in context.
    /// </summary>
    public string? Actor { get; }

    /// <summary>
    /// Gets the job in context.
    /// </summary>
    public string? Job { get; }

    /// <summary>
    /// Gets the run attempt number in context.
    /// </summary>
    public long RunAttempt { get; }

    /// <summary>
    /// Gets the run number in context.
    /// </summary>
    public long RunNumber { get; }

    /// <summary>
    /// Gets the run id in context.
    /// </summary>
    public long RunId { get; }

    /// <summary>
    /// Gets the API URL in context.
    /// </summary>
    public string ApiUrl { get; }

    /// <summary>
    /// Gets the server URL in context.
    /// </summary>
    public string ServerUrl { get; }

    /// <summary>
    /// Gets the GraphQL URL in context.
    /// </summary>
    public string GraphQlUrl { get; }

    /// <summary>
    /// Gets the repo in context.
    /// </summary>
    public Repository Repo { get; }

    /// <summary>
    /// Gets the issue in context.
    /// </summary>
    public Issue Issue { get; }

    /// <summary>
    /// For testing purposes only!
    /// The <see cref="Payload"/>, <see cref="Issue"/>, and
    /// <see cref="Repo"/> are the only objects that are
    /// deserialized when using this approach. All other members are
    /// their <see langword="default" /> value.
    /// </summary>
    public static Context? FromJson(
        [StringSyntax(StringSyntaxAttribute.Json)] string? json)
    {
        return json switch
        {
            null or { Length: 0 } => null,

            _ => JsonSerializer.Deserialize(
                json,
                SourceGenerationContexts.Default.Context)
        };
    }

    [JsonConstructor]
    internal Context(WebhookPayload? payload)
    {
        ApiUrl = "https://api.github.com";
        ServerUrl = "https://github.com";
        GraphQlUrl = "https://api.github.com/graphql";

        Payload = payload;

        if (Payload is { Repository: { } })
        {
            Issue = new(
                Payload.Repository.Owner.Login,
                Payload.Repository.Name,
                Payload.Issue?.Number ??
                    Payload.PullRequest?.Number ?? 0);

            Repo = new Repository(
                Payload.Repository.Owner.Login,
                Payload.Repository.Name);
        }
    }

    private Context()
    {
        var eventPath = GetEnvironmentVariable(GITHUB_EVENT_PATH);
        if (!string.IsNullOrWhiteSpace(eventPath) && File.Exists(eventPath))
        {
            var json = File.ReadAllText(eventPath, Encoding.UTF8);

            Payload = JsonSerializer.Deserialize(
                json, SourceGenerationContexts.Default.WebhookPayload)!;
        }
        else
        {
            Console.WriteLine($"GITHUB_EVENT_PATH ${eventPath} does not exist");
        }

        EventName = GetEnvironmentVariable(GITHUB_EVENT_NAME);
        Sha = GetEnvironmentVariable(GITHUB_SHA);
        Ref = GetEnvironmentVariable(GITHUB_REF);
        Workflow = GetEnvironmentVariable(GITHUB_WORKFLOW);
        Action = GetEnvironmentVariable(GITHUB_ACTION);
        Actor = GetEnvironmentVariable(GITHUB_ACTOR);
        Job = GetEnvironmentVariable(GITHUB_JOB);
        RunAttempt = long.TryParse(GetEnvironmentVariable(GITHUB_RUN_ATTEMPT), out var attempt) ? attempt : 10;
        RunNumber = long.TryParse(GetEnvironmentVariable(GITHUB_RUN_NUMBER), out var number) ? number : 10;
        RunId = long.TryParse(GetEnvironmentVariable(GITHUB_RUN_ID), out var id) ? id : 10;
        ApiUrl = GetEnvironmentVariable(GITHUB_API_URL) ?? "https://api.github.com";
        ServerUrl = GetEnvironmentVariable(GITHUB_SERVER_URL) ?? "https://github.com";
        GraphQlUrl = GetEnvironmentVariable(GITHUB_GRAPHQL_URL) ?? "https://api.github.com/graphql";

        if (Payload is { Repository: { } })
        {
            Issue = new(
                Payload.Repository.Owner.Login,
                Payload.Repository.Name,
                Payload.Issue?.Number ??
                    Payload.PullRequest?.Number ?? 0);
        }

        var repository = GetEnvironmentVariable(GITHUB_REPOSITORY);
        if (!string.IsNullOrWhiteSpace(repository))
        {
            var parts = repository.Split('/');
            if (parts is { Length: 2 })
            {
                Repo = new Repository(parts[0], parts[1]);
            }
        }
        else if (Payload is { Repository: { } })
        {
            Repo = new Repository(
                Payload.Repository.Owner.Login,
                Payload.Repository.Name);
        }
    }

    /// <summary>
    /// Gets a JSON representation of the current context.
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(
            this,
            SourceGenerationContexts.Default.Context);
    }
}
