// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Sigstore;

namespace ActionsToolkit.Attest.Auth;

/// <summary>
/// <see cref="IOidcTokenProvider"/> implementation that exchanges the GitHub
/// Actions runtime OIDC envelope (<c>ACTIONS_ID_TOKEN_REQUEST_URL</c> +
/// <c>ACTIONS_ID_TOKEN_REQUEST_TOKEN</c>) for a JWT scoped to the
/// <c>sigstore</c> audience. Mirrors the
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/sign.ts">
/// <c>CIContextProvider('sigstore')</c></a> behavior used by the upstream
/// <c>@actions/attest</c> package.
/// </summary>
public sealed class GitHubActionsOidcTokenProvider : IOidcTokenProvider
{
    /// <summary>
    /// The OIDC audience expected by Sigstore's Fulcio instance.
    /// </summary>
    internal const string SigstoreAudience = "sigstore";

    /// <summary>
    /// Environment variable holding the per-job OIDC request URL.
    /// </summary>
    internal const string RequestUrlEnvironmentVariable = "ACTIONS_ID_TOKEN_REQUEST_URL";

    /// <summary>
    /// Environment variable holding the per-job OIDC request bearer token.
    /// </summary>
    internal const string RequestTokenEnvironmentVariable = "ACTIONS_ID_TOKEN_REQUEST_TOKEN";

    private readonly NetClient _httpClient;
    private readonly string _audience;
    private readonly Func<string, string?> _environmentReader;

    /// <summary>
    /// Creates a new <see cref="GitHubActionsOidcTokenProvider"/> using the
    /// real process environment and a freshly-allocated
    /// <see cref="NetClient"/>.
    /// </summary>
    public GitHubActionsOidcTokenProvider()
        : this(new NetClient(), SigstoreAudience, Environment.GetEnvironmentVariable)
    {
    }

    /// <summary>
    /// Creates a new <see cref="GitHubActionsOidcTokenProvider"/> bound to the
    /// supplied <paramref name="httpClient"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to contact the OIDC
    /// minting endpoint. The instance is not disposed by this provider.</param>
    public GitHubActionsOidcTokenProvider(NetClient httpClient)
        : this(httpClient, SigstoreAudience, Environment.GetEnvironmentVariable)
    {
    }

    /// <summary>
    /// Test-friendly constructor that allows overriding both the
    /// <paramref name="httpClient"/> and the
    /// <paramref name="environmentReader"/>.
    /// </summary>
    internal GitHubActionsOidcTokenProvider(
        NetClient httpClient,
        string audience,
        Func<string, string?> environmentReader)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
        ArgumentNullException.ThrowIfNull(environmentReader);

        _httpClient = httpClient;
        _audience = audience;
        _environmentReader = environmentReader;
    }

    /// <inheritdoc />
    public async Task<OidcToken> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var requestUrl = _environmentReader(RequestUrlEnvironmentVariable);
        var requestToken = _environmentReader(RequestTokenEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(requestUrl))
        {
            throw new InvalidOperationException(
                $"Environment variable '{RequestUrlEnvironmentVariable}' is not set; " +
                "this provider requires a GitHub Actions OIDC envelope.");
        }

        if (string.IsNullOrWhiteSpace(requestToken))
        {
            throw new InvalidOperationException(
                $"Environment variable '{RequestTokenEnvironmentVariable}' is not set; " +
                "this provider requires a GitHub Actions OIDC envelope.");
        }

        var separator = requestUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        var fullUrl = $"{requestUrl}{separator}audience={Uri.EscapeDataString(_audience)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("bearer", requestToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Failed to mint OIDC token: {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        var payload = await response.Content
            .ReadFromJsonAsync(AttestJsonContext.Default.OidcTokenResponse, cancellationToken)
            .ConfigureAwait(false);

        if (payload is null || string.IsNullOrWhiteSpace(payload.Value))
        {
            throw new InvalidOperationException(
                "OIDC token response did not contain a 'value' field.");
        }

        return new OidcToken
        {
            RawToken = payload.Value,
            Subject = string.Empty,
            Issuer = string.Empty,
        };
    }
}

/// <summary>
/// Wire shape of the JSON returned by the GitHub Actions OIDC minting
/// endpoint.
/// </summary>
internal sealed class OidcTokenResponse
{
    /// <summary>
    /// The minted JWT.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
