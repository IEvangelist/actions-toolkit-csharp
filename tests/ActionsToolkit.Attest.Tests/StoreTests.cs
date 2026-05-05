// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Octokit;

namespace ActionsToolkit.Attest.Tests;

/// <summary>
/// Mirrors the upstream <c>__tests__/store.test.ts</c> suite.
/// </summary>
public class StoreTests
{
    private const string Token = "ghs_token";

    [Fact(DisplayName = "persists the attestation and returns the attestation id")]
    public async Task PersistsAttestationAndReturnsId()
    {
        var bundle = JsonNode.Parse("""{ "mediaType": "application/vnd.dev.sigstore.bundle.v0.3+json" }""")!;
        JsonNode? capturedBody = null;

        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://api.github.com/repos/octocat/Hello-World/attestations")
            .WithHeaders("Authorization", $"token {Token}")
            .WithHeaders("X-Custom", "yes")
            .Respond(async req =>
            {
                var body = await req.Content!.ReadAsStringAsync().ConfigureAwait(false);
                capturedBody = JsonNode.Parse(body);
                return new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("""{ "id": 12345 }""", Encoding.UTF8, "application/json"),
                };
            });

        var store = CreateStore(mock, owner: "octocat", repo: "Hello-World");
        var headers = new Dictionary<string, string> { ["X-Custom"] = "yes" };

        var id = await store.WriteAsync(bundle, Token, headers).ConfigureAwait(true);

        Assert.Equal("12345", id);
        Assert.NotNull(capturedBody);
        Assert.Equal(
            "application/vnd.dev.sigstore.bundle.v0.3+json",
            capturedBody!["bundle"]!["mediaType"]!.GetValue<string>());
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact(DisplayName = "throws when the attestations endpoint returns a non-success status")]
    public async Task ThrowsOnNonSuccessStatus()
    {
        var bundle = JsonNode.Parse("""{ "ok": true }""")!;
        var mock = new MockHttpMessageHandler();
        mock.When("https://api.github.com/repos/octocat/Hello-World/attestations")
            .Respond(HttpStatusCode.UnprocessableEntity, new StringContent("nope"));

        var store = CreateStore(mock, owner: "octocat", repo: "Hello-World");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.WriteAsync(bundle, Token)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "throws when the repository owner/repo is not set")]
    public async Task ThrowsWhenRepoMissing()
    {
        var bundle = JsonNode.Parse("""{ "ok": true }""")!;
        var mock = new MockHttpMessageHandler();
        var store = CreateStore(mock, owner: null, repo: null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.WriteAsync(bundle, Token)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "throws when the token is null or whitespace")]
    public async Task ThrowsWhenTokenMissing()
    {
        var bundle = JsonNode.Parse("""{ "ok": true }""")!;
        var mock = new MockHttpMessageHandler();
        var store = CreateStore(mock, owner: "owner", repo: "repo");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            store.WriteAsync(bundle, " ")).ConfigureAwait(true);
    }

    private static GitHubAttestationStore CreateStore(MockHttpMessageHandler handler, string? owner, string? repo)
    {
        var services = new ServiceCollection();
        services.AddHttpClient(GitHubAttestationStore.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        services.AddSingleton<IGitHubClientFactory, DefaultGitHubClientFactory>();

        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IHttpClientFactory>();
        var clientFactory = sp.GetRequiredService<IGitHubClientFactory>();

        var contextJson = (owner is null || repo is null)
            ? "{ \"payload\": null }"
            : $$"""
            {
              "payload": {
                "repository": {
                  "name": "{{repo}}",
                  "owner": { "login": "{{owner}}" }
                }
              }
            }
            """;

        var context = Context.FromJson(contextJson)
            ?? throw new InvalidOperationException("Failed to build a Context from the test JSON.");

        return new GitHubAttestationStore(factory, context, clientFactory);
    }
}
