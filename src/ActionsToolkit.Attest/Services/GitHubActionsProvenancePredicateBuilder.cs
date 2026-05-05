// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Services;

/// <summary>
/// Default <see cref="IProvenancePredicateBuilder"/> that decodes the
/// per-job GitHub Actions OIDC token (audience: <c>nobody</c>) and assembles
/// an SLSA v1 build provenance predicate populated with the
/// <c>github-actions-buildtypes/workflow/v1</c> external/internal parameters.
/// </summary>
internal sealed class GitHubActionsProvenancePredicateBuilder : IProvenancePredicateBuilder
{
    /// <summary>
    /// The SLSA build provenance v1 predicate type URI.
    /// </summary>
    internal const string SlsaPredicateV1Type = "https://slsa.dev/provenance/v1";

    /// <summary>
    /// The GitHub Actions Workflow build type URI.
    /// </summary>
    internal const string GitHubBuildType = "https://actions.github.io/buildtypes/workflow/v1";

    /// <summary>
    /// The OIDC audience used by <c>buildSLSAProvenancePredicate</c>.
    /// </summary>
    internal const string ProvenanceAudience = "nobody";

    private const string GitHubServerUrlEnvironmentVariable = "GITHUB_SERVER_URL";

    private readonly Func<string, CancellationToken, Task<string>> _tokenFetcher;
    private readonly Func<string, string?> _environmentReader;

    /// <summary>
    /// Default constructor used by DI. Hydrates against
    /// <c>ACTIONS_ID_TOKEN_REQUEST_URL</c> /
    /// <c>ACTIONS_ID_TOKEN_REQUEST_TOKEN</c> via a fresh
    /// <see cref="GitHubActionsOidcTokenProvider"/>.
    /// </summary>
    public GitHubActionsProvenancePredicateBuilder()
        : this(DefaultTokenFetcher, Environment.GetEnvironmentVariable)
    {
    }

    /// <summary>
    /// Test-friendly constructor allowing the OIDC token fetch and environment
    /// reads to be stubbed.
    /// </summary>
    internal GitHubActionsProvenancePredicateBuilder(
        Func<string, CancellationToken, Task<string>> tokenFetcher,
        Func<string, string?> environmentReader)
    {
        ArgumentNullException.ThrowIfNull(tokenFetcher);
        ArgumentNullException.ThrowIfNull(environmentReader);

        _tokenFetcher = tokenFetcher;
        _environmentReader = environmentReader;
    }

    public async Task<Predicate> BuildAsync(string? issuer, CancellationToken cancellationToken = default)
    {
        // The upstream code derives the issuer from GITHUB_SERVER_URL; we don't
        // need it once we have a token in hand. We do still pass it through to
        // the fetcher so a custom issuer can be honored by alternate fetchers.
        var jwt = await _tokenFetcher(issuer ?? string.Empty, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(jwt))
        {
            throw new InvalidOperationException("OIDC token fetcher returned an empty JWT.");
        }

        var claims = DecodeClaims(jwt);

        var serverUrl = _environmentReader(GitHubServerUrlEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            serverUrl = "https://github.com";
        }

        var repository = RequireString(claims, "repository");
        var workflowRef = RequireString(claims, "workflow_ref");

        // owner/repo/.github/workflows/main.yml@main =>
        //   .github/workflows/main.yml, main
        var withoutRepoPrefix = workflowRef.StartsWith($"{repository}/", StringComparison.Ordinal)
            ? workflowRef[(repository.Length + 1)..]
            : workflowRef;
        var atIndex = withoutRepoPrefix.IndexOf('@', StringComparison.Ordinal);
        var workflowPath = atIndex >= 0 ? withoutRepoPrefix[..atIndex] : withoutRepoPrefix;

        var refClaim = RequireString(claims, "ref");
        var sha = RequireString(claims, "sha");
        var jobWorkflowRef = RequireString(claims, "job_workflow_ref");
        var runId = RequireString(claims, "run_id");
        var runAttempt = RequireString(claims, "run_attempt");

        var predicateParams = new JsonObject
        {
            ["buildDefinition"] = new JsonObject
            {
                ["buildType"] = GitHubBuildType,
                ["externalParameters"] = new JsonObject
                {
                    ["workflow"] = new JsonObject
                    {
                        ["ref"] = refClaim,
                        ["repository"] = $"{serverUrl}/{repository}",
                        ["path"] = workflowPath,
                    },
                },
                ["internalParameters"] = new JsonObject
                {
                    ["github"] = new JsonObject
                    {
                        ["event_name"] = RequireString(claims, "event_name"),
                        ["repository_id"] = RequireString(claims, "repository_id"),
                        ["repository_owner_id"] = RequireString(claims, "repository_owner_id"),
                        ["runner_environment"] = RequireString(claims, "runner_environment"),
                    },
                },
                ["resolvedDependencies"] = new JsonArray(
                    new JsonObject
                    {
                        ["uri"] = $"git+{serverUrl}/{repository}@{refClaim}",
                        ["digest"] = new JsonObject
                        {
                            ["gitCommit"] = sha,
                        },
                    }),
            },
            ["runDetails"] = new JsonObject
            {
                ["builder"] = new JsonObject
                {
                    ["id"] = $"{serverUrl}/{jobWorkflowRef}",
                },
                ["metadata"] = new JsonObject
                {
                    ["invocationId"] = $"{serverUrl}/{repository}/actions/runs/{runId}/attempts/{runAttempt}",
                },
            },
        };

        return new Predicate
        {
            Type = SlsaPredicateV1Type,
            Params = predicateParams,
        };
    }

    private static string RequireString(JsonObject claims, string name)
    {
        var node = claims[name];
        if (node is null)
        {
            throw new InvalidOperationException($"OIDC claim '{name}' is missing.");
        }

        var value = node.GetValue<string>();
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"OIDC claim '{name}' is empty.");
        }

        return value;
    }

    /// <summary>
    /// Decodes the JWT payload (middle segment) without performing signature
    /// verification — same behavior as the upstream <c>decodeOIDCToken</c>
    /// after JWKS verification has succeeded. AOT-friendly: parses with
    /// <see cref="JsonNode"/> rather than reflection-based deserialization.
    /// </summary>
    internal static JsonObject DecodeClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            throw new InvalidOperationException("OIDC token is not a JWT.");
        }

        var payload = parts[1];
        // Pad base64url
        var padded = payload.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        var bytes = Convert.FromBase64String(padded);
        var node = JsonNode.Parse(bytes)
            ?? throw new InvalidOperationException("OIDC payload could not be parsed as JSON.");

        return node as JsonObject
            ?? throw new InvalidOperationException("OIDC payload is not a JSON object.");
    }

    private static async Task<string> DefaultTokenFetcher(string _, CancellationToken cancellationToken)
    {
        using var http = new NetClient();
        var provider = new GitHubActionsOidcTokenProvider(
            http,
            ProvenanceAudience,
            Environment.GetEnvironmentVariable);
        var token = await provider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        return token.RawToken;
    }
}
