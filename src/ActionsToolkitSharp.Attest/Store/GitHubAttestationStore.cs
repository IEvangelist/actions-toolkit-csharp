// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Store;

/// <summary>
/// Default <see cref="IAttestationStore"/> that POSTs the bundle to
/// <c>POST {GITHUB_API_URL}/repos/{owner}/{repo}/attestations</c>. The
/// <see cref="IGitHubClientFactory"/> is consulted for token-bound Octokit
/// client construction (kept as an injected dependency to satisfy the DI
/// surface even though the actual transport here is a raw
/// <see cref="NetClient"/> POST — this matches the upstream behavior of
/// <c>octokit.request(CREATE_ATTESTATION_REQUEST, …)</c> which is itself a
/// raw REST call).
/// </summary>
internal sealed class GitHubAttestationStore : IAttestationStore
{
    /// <summary>
    /// Named <see cref="IHttpClientFactory"/> client used by the store.
    /// </summary>
    internal const string HttpClientName = "ActionsToolkitSharp.Attest";

    /// <summary>
    /// Default User-Agent value sent on attestation POSTs.
    /// </summary>
    internal const string UserAgent = "ActionsToolkitSharp.Attest";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Context _context;
    private readonly IGitHubClientFactory _clientFactory;

    public GitHubAttestationStore(
        IHttpClientFactory httpClientFactory,
        Context context,
        IGitHubClientFactory clientFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(clientFactory);

        _httpClientFactory = httpClientFactory;
        _context = context;
        _clientFactory = clientFactory;
    }

    public async Task<string?> WriteAsync(
        JsonNode bundle,
        string token,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        if (string.IsNullOrWhiteSpace(_context.Repo.Owner) ||
            string.IsNullOrWhiteSpace(_context.Repo.Repo))
        {
            throw new InvalidOperationException(
                "Repository owner/repo is not set in Context (GITHUB_REPOSITORY).");
        }

        // Eagerly construct a per-call Octokit client so the IGitHubClientFactory
        // contract is exercised and the token is validated up front. We do not
        // use the typed Kiota surface for the attestations POST because the
        // upstream wire format is raw bundle JSON under a single 'bundle' key
        // — see actions/toolkit:packages/attest/src/store.ts.
        _ = _clientFactory.Create(token);

        var apiBase = string.IsNullOrWhiteSpace(_context.ApiUrl)
            ? "https://api.github.com"
            : _context.ApiUrl;

        var url = $"{apiBase.TrimEnd('/')}/repos/{_context.Repo.Owner}/{_context.Repo.Repo}/attestations";

        var http = _httpClientFactory.CreateClient(HttpClientName);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("token", token);
        request.Headers.UserAgent.ParseAdd(UserAgent);

        if (headers is not null)
        {
            foreach (var (key, value) in headers)
            {
                request.Headers.TryAddWithoutValidation(key, value);
            }
        }

        var bodyDoc = new JsonObject { ["bundle"] = bundle.DeepClone() };
        var json = bodyDoc.ToJsonString(AttestJsonContext.Default.Options);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Failed to persist attestation: {(int)response.StatusCode} {response.ReasonPhrase}: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            var node = JsonNode.Parse(responseBody);
            return node?["id"]?.ToString();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
