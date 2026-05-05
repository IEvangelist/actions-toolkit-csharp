// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// Lightweight, AOT-clean wrapper around the small subset of the GitHub
/// public REST API the artifact client needs for cross-workflow
/// (<see cref="FindBy"/>) operations. Avoids taking a dependency on the
/// generated <c>GitHub.Octokit.SDK</c> for these handful of endpoints —
/// the wire shapes are stable and source-gen JSON keeps the entire path
/// AOT-compatible.
/// </summary>
internal interface IPublicArtifactsApi
{
    Task<IReadOnlyList<Artifact>> ListAsync(
        FindBy findBy,
        string? nameFilter,
        CancellationToken cancellationToken);

    Task<Uri> GetDownloadRedirectAsync(
        FindBy findBy,
        long artifactId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        FindBy findBy,
        long artifactId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Default <see cref="IPublicArtifactsApi"/> implemented over the named
/// <see cref="IHttpClientFactory"/> client registered by
/// <c>AddGitHubActionsArtifact</c>. The client is created without an
/// authorization header — each call attaches the caller's token from
/// <see cref="FindBy.Token"/> on the request itself, since findBy tokens
/// are typically per-call.
/// </summary>
internal sealed class DefaultPublicArtifactsApi : IPublicArtifactsApi
{
    /// <summary>
    /// The named <see cref="IHttpClientFactory"/> client used for the public
    /// REST API calls. Configured by <c>AddGitHubActionsArtifact</c> with
    /// the GitHub API base URL and standard resilience handler.
    /// </summary>
    internal const string HttpClientName = "ActionsToolkit.Artifact.PublicApi";

    private const string ApiVersionHeader = "X-GitHub-Api-Version";
    private const string ApiVersionValue = "2022-11-28";

    private static PublicArtifactsJsonContext JsonContext => PublicArtifactsJsonContext.Default;

    private readonly IHttpClientFactory _factory;

    public DefaultPublicArtifactsApi(IHttpClientFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    public async Task<IReadOnlyList<Artifact>> ListAsync(
        FindBy findBy,
        string? nameFilter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(findBy);

        var client = _factory.CreateClient(HttpClientName);

        var path = $"repos/{Uri.EscapeDataString(findBy.RepositoryOwner)}"
                   + $"/{Uri.EscapeDataString(findBy.RepositoryName)}"
                   + $"/actions/runs/{findBy.WorkflowRunId.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                   + "/artifacts?per_page=100";
        if (!string.IsNullOrEmpty(nameFilter))
        {
            path += $"&name={Uri.EscapeDataString(nameFilter)}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        ConfigureRequest(request, findBy.Token);

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidArtifactResponseException(
                $"List artifacts: GitHub REST API returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        var payload = await response.Content
            .ReadFromJsonAsync(JsonContext.PublicArtifactsListResponse, cancellationToken)
            .ConfigureAwait(false);

        if (payload is null)
        {
            throw new InvalidArtifactResponseException(
                "List artifacts: GitHub REST API returned an empty response body.");
        }

        var artifacts = new Artifact[payload.Artifacts.Length];
        for (var i = 0; i < payload.Artifacts.Length; i++)
        {
            var item = payload.Artifacts[i];
            artifacts[i] = new Artifact(item.Name, item.Id, item.SizeInBytes, item.CreatedAt, item.Digest);
        }

        return artifacts;
    }

    public async Task<Uri> GetDownloadRedirectAsync(
        FindBy findBy,
        long artifactId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(findBy);

        var client = _factory.CreateClient(HttpClientName);

        var path = $"repos/{Uri.EscapeDataString(findBy.RepositoryOwner)}"
                   + $"/{Uri.EscapeDataString(findBy.RepositoryName)}"
                   + $"/actions/artifacts/{artifactId.ToString(System.Globalization.CultureInfo.InvariantCulture)}/zip";

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        ConfigureRequest(request, findBy.Token);

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        // The GitHub REST API responds with a 302 redirect carrying the
        // signed blob URL in the Location header. We do not want HttpClient
        // to follow the redirect because the signed URL must not receive the
        // GitHub bearer token.
        if (response.StatusCode != System.Net.HttpStatusCode.Found &&
            response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidArtifactResponseException(
                $"Download artifact: GitHub REST API returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        var location = response.Headers.Location;
        if (location is null)
        {
            throw new InvalidArtifactResponseException(
                "Download artifact: GitHub REST API redirect did not include a Location header.");
        }

        return location;
    }

    public async Task DeleteAsync(
        FindBy findBy,
        long artifactId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(findBy);

        var client = _factory.CreateClient(HttpClientName);

        var path = $"repos/{Uri.EscapeDataString(findBy.RepositoryOwner)}"
                   + $"/{Uri.EscapeDataString(findBy.RepositoryName)}"
                   + $"/actions/artifacts/{artifactId.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, path);
        ConfigureRequest(request, findBy.Token);

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != System.Net.HttpStatusCode.NoContent &&
            !response.IsSuccessStatusCode)
        {
            throw new InvalidArtifactResponseException(
                $"Delete artifact: GitHub REST API returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }
    }

    private static void ConfigureRequest(HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.TryAddWithoutValidation(ApiVersionHeader, ApiVersionValue);
    }
}
