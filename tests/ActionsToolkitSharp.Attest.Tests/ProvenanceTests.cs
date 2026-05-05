// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Tests;

/// <summary>
/// Mirrors the upstream <c>__tests__/provenance.test.ts</c> suite.
/// </summary>
public class ProvenanceTests
{
    [Fact(DisplayName = "returns a SLSA v1 provenance predicate hydrated from an OIDC token")]
    public async Task ReturnsHydratedProvenancePredicate()
    {
        var jwt = TestData.EncodeFakeJwt(TestData.DefaultClaims());

        var builder = new GitHubActionsProvenancePredicateBuilder(
            (_, _) => Task.FromResult(jwt),
            _ => "https://github.com");

        var predicate = await builder.BuildAsync(issuer: null).ConfigureAwait(true);

        Assert.Equal("https://slsa.dev/provenance/v1", predicate.Type);
        var ext = predicate.Params["buildDefinition"]!["externalParameters"]!["workflow"]!.AsObject();
        Assert.Equal("refs/heads/main", ext["ref"]!.GetValue<string>());
        Assert.Equal("https://github.com/owner/repo", ext["repository"]!.GetValue<string>());
        Assert.Equal(".github/workflows/main.yml", ext["path"]!.GetValue<string>());

        var ghInternal = predicate.Params["buildDefinition"]!["internalParameters"]!["github"]!.AsObject();
        Assert.Equal("push", ghInternal["event_name"]!.GetValue<string>());
        Assert.Equal("1", ghInternal["repository_id"]!.GetValue<string>());

        var resolved = predicate.Params["buildDefinition"]!["resolvedDependencies"]!.AsArray()[0]!.AsObject();
        Assert.Equal("git+https://github.com/owner/repo@refs/heads/main", resolved["uri"]!.GetValue<string>());
        Assert.Equal("babca52ab0c93ae16539e5923cb0d7403b9a093b", resolved["digest"]!["gitCommit"]!.GetValue<string>());

        var meta = predicate.Params["runDetails"]!["metadata"]!.AsObject();
        Assert.EndsWith("/owner/repo/actions/runs/12345/attempts/1", meta["invocationId"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "throws when the OIDC token is missing required claims")]
    public async Task ThrowsWhenClaimsMissing()
    {
        var claims = new JsonObject
        {
            ["iss"] = "https://token.actions.githubusercontent.com",
        };

        var jwt = TestData.EncodeFakeJwt(claims);
        var builder = new GitHubActionsProvenancePredicateBuilder(
            (_, _) => Task.FromResult(jwt),
            _ => "https://github.com");

        await Assert.ThrowsAsync<InvalidOperationException>(() => builder.BuildAsync(null)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "throws when the JWT body is malformed")]
    public async Task ThrowsOnMalformedJwt()
    {
        var builder = new GitHubActionsProvenancePredicateBuilder(
            (_, _) => Task.FromResult("not-a-jwt"),
            _ => "https://github.com");

        await Assert.ThrowsAsync<InvalidOperationException>(() => builder.BuildAsync(null)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "attests provenance via DefaultAttestService.AttestProvenanceAsync")]
    public async Task AttestsProvenanceEndToEnd()
    {
        var signer = new StubAttestSigner();
        var store = new StubAttestationStore("prov-1");
        var predicateBuilder = new StubProvenancePredicateBuilder();
        var sut = new DefaultAttestService(signer, store, predicateBuilder);

        var result = await sut.AttestProvenanceAsync(new AttestProvenanceOptions
        {
            SubjectName = TestData.SubjectName,
            SubjectDigest = TestData.SubjectDigest,
            Token = "fake",
        }).ConfigureAwait(true);

        Assert.Equal("prov-1", result.AttestationId);
        var statement = JsonNode.Parse(signer.LastStatementJson!)!.AsObject();
        Assert.Equal("https://slsa.dev/provenance/v1", statement["predicateType"]!.GetValue<string>());
    }

    [Fact(DisplayName = "honors GITHUB_SERVER_URL override when building the provenance predicate")]
    public async Task HonorsServerUrlOverride()
    {
        var jwt = TestData.EncodeFakeJwt(TestData.DefaultClaims());
        var builder = new GitHubActionsProvenancePredicateBuilder(
            (_, _) => Task.FromResult(jwt),
            _ => "https://my.ghe.com");

        var predicate = await builder.BuildAsync(null).ConfigureAwait(true);
        var ext = predicate.Params["buildDefinition"]!["externalParameters"]!["workflow"]!.AsObject();
        Assert.Equal("https://my.ghe.com/owner/repo", ext["repository"]!.GetValue<string>());
    }
}
