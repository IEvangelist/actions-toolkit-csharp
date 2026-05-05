// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Tests;

/// <summary>
/// Mirrors the upstream <c>__tests__/oidc.test.ts</c> suite for the GitHub
/// Actions OIDC envelope handling.
/// </summary>
public class OidcTests
{
    private const string MintEndpoint = "https://example.test/oidc";
    private const string EnvelopeBearer = "envelope-bearer";

    [Fact(DisplayName = "returns the minted JWT value")]
    public async Task ReturnsMintedTokenValue()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .Expect(HttpMethod.Get, $"{MintEndpoint}?audience=sigstore")
            .WithHeaders("Authorization", $"bearer {EnvelopeBearer}")
            .Respond("application/json", """{ "value": "minted.jwt.value" }""");

        using var http = new System.Net.Http.HttpClient(handler);
        var provider = new GitHubActionsOidcTokenProvider(http, "sigstore", EnvReader);

        var token = await provider.GetTokenAsync().ConfigureAwait(true);

        Assert.Equal("minted.jwt.value", token.RawToken);
        handler.VerifyNoOutstandingExpectation();
    }

    [Fact(DisplayName = "appends 'audience' as the only query parameter when none exist")]
    public async Task AppendsAudienceQueryParameter()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .Expect(HttpMethod.Get, $"{MintEndpoint}?audience=nobody")
            .Respond("application/json", """{ "value": "ok" }""");

        using var http = new System.Net.Http.HttpClient(handler);
        var provider = new GitHubActionsOidcTokenProvider(http, "nobody", EnvReader);

        var token = await provider.GetTokenAsync().ConfigureAwait(true);

        Assert.Equal("ok", token.RawToken);
    }

    [Fact(DisplayName = "appends 'audience' with '&' when query string already present")]
    public async Task AppendsAudienceWithAmpersand()
    {
        var handler = new MockHttpMessageHandler();
        handler
            .Expect(HttpMethod.Get, $"{MintEndpoint}?token=existing&audience=sigstore")
            .Respond("application/json", """{ "value": "ok" }""");

        using var http = new System.Net.Http.HttpClient(handler);
        var provider = new GitHubActionsOidcTokenProvider(
            http,
            "sigstore",
            key => key switch
            {
                "ACTIONS_ID_TOKEN_REQUEST_URL" => $"{MintEndpoint}?token=existing",
                "ACTIONS_ID_TOKEN_REQUEST_TOKEN" => EnvelopeBearer,
                _ => null,
            });

        var token = await provider.GetTokenAsync().ConfigureAwait(true);
        Assert.Equal("ok", token.RawToken);
    }

    [Fact(DisplayName = "throws when ACTIONS_ID_TOKEN_REQUEST_URL is unset")]
    public async Task ThrowsWhenRequestUrlMissing()
    {
        using var http = new System.Net.Http.HttpClient(new MockHttpMessageHandler());
        var provider = new GitHubActionsOidcTokenProvider(http, "sigstore", _ => null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetTokenAsync()).ConfigureAwait(true);
    }

    [Fact(DisplayName = "throws when ACTIONS_ID_TOKEN_REQUEST_TOKEN is unset")]
    public async Task ThrowsWhenRequestTokenMissing()
    {
        using var http = new System.Net.Http.HttpClient(new MockHttpMessageHandler());
        var provider = new GitHubActionsOidcTokenProvider(
            http,
            "sigstore",
            key => key == "ACTIONS_ID_TOKEN_REQUEST_URL" ? MintEndpoint : null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetTokenAsync()).ConfigureAwait(true);
    }

    [Fact(DisplayName = "throws when the OIDC mint endpoint returns a non-success status")]
    public async Task ThrowsWhenMintEndpointFails()
    {
        var handler = new MockHttpMessageHandler();
        handler.When(MintEndpoint + "*").Respond(HttpStatusCode.InternalServerError);

        using var http = new System.Net.Http.HttpClient(handler);
        var provider = new GitHubActionsOidcTokenProvider(http, "sigstore", EnvReader);

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetTokenAsync()).ConfigureAwait(true);
    }

    [Fact(DisplayName = "throws when the OIDC mint response has no 'value' field")]
    public async Task ThrowsWhenValueFieldMissing()
    {
        var handler = new MockHttpMessageHandler();
        handler.When(MintEndpoint + "*").Respond("application/json", "{ }");

        using var http = new System.Net.Http.HttpClient(handler);
        var provider = new GitHubActionsOidcTokenProvider(http, "sigstore", EnvReader);

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetTokenAsync()).ConfigureAwait(true);
    }

    private static string? EnvReader(string key) => key switch
    {
        "ACTIONS_ID_TOKEN_REQUEST_URL" => MintEndpoint,
        "ACTIONS_ID_TOKEN_REQUEST_TOKEN" => EnvelopeBearer,
        _ => null,
    };
}
