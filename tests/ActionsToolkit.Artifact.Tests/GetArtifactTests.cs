// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/get-artifact.test.ts</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class GetArtifactTests : IDisposable
{
    private readonly EnvironmentScope _serverScope =
        new("GITHUB_SERVER_URL", "https://github.com");

    public void Dispose() => _serverScope.Dispose();

    [Fact(DisplayName = "Throws GhesNotSupportedException on a GHES host")]
    public async Task ThrowsOnGhes()
    {
        using var ghesScope = new EnvironmentScope("GITHUB_SERVER_URL", "https://my-ghes.example.com");

        var client = BuildClient(out _, out _);

        await Assert.ThrowsAsync<GhesNotSupportedException>(
            () => client.GetArtifactAsync("name").AsTask());
    }

    [Fact(DisplayName = "Throws ArtifactNotFoundException when no artifact matches")]
    public async Task ThrowsWhenNotFound()
    {
        var client = BuildClient(out var service, out _);
        service.OnList = _ => new Internal.Twirp.ListArtifactsResponse
        {
            Artifacts = [],
        };

        await Assert.ThrowsAsync<ArtifactNotFoundException>(
            () => client.GetArtifactAsync("missing").AsTask());
    }

    [Fact(DisplayName = "Returns the highest-id artifact when multiple share a name")]
    public async Task ReturnsHighestIdOnDuplicate()
    {
        var client = BuildClient(out var service, out _);
        service.OnList = _ => new Internal.Twirp.ListArtifactsResponse
        {
            Artifacts =
            [
                new MonolithArtifact
                {
                    WorkflowRunBackendId = "run-1",
                    WorkflowJobRunBackendId = "job-1",
                    DatabaseId = 3,
                    Name = "name",
                    Size = 10,
                },
                new MonolithArtifact
                {
                    WorkflowRunBackendId = "run-1",
                    WorkflowJobRunBackendId = "job-1",
                    DatabaseId = 11,
                    Name = "name",
                    Size = 20,
                },
            ],
        };

        var response = await client.GetArtifactAsync("name");

        Assert.Equal(11, response.Artifact.Id);
    }

    [Fact(DisplayName = "FindBy routes through the public REST API")]
    public async Task FindByRoutesThroughRest()
    {
        var client = BuildClient(out var service, out var publicApi);
        var findBy = new FindBy("token", 99, "owner", "repo");
        publicApi.OnList = (_, name) =>
        [
            new Artifact(name ?? "x", 5, 100, DateTimeOffset.UtcNow),
        ];

        var response = await client.GetArtifactAsync("name", new GetArtifactOptions { FindBy = findBy });

        Assert.Empty(service.ListRequests);
        Assert.Single(publicApi.ListInvocations);
        Assert.Equal("name", response.Artifact.Name);
        Assert.Equal(5, response.Artifact.Id);
    }

    [Fact(DisplayName = "Throws ArgumentException when artifactName is null or empty")]
    public async Task ThrowsForNullOrEmptyName()
    {
        var client = BuildClient(out _, out _);

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.GetArtifactAsync(string.Empty).AsTask());
    }

    private static DefaultArtifactClient BuildClient(
        out FakeArtifactService service,
        out FakePublicArtifactsApi publicApi)
    {
        service = new FakeArtifactService();
        publicApi = new FakePublicArtifactsApi();
        var factory = new TestHttpClientFactory(new TestHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK)));
        return new DefaultArtifactClient(service, new FakeBackendIdsProvider(), factory, publicApi);
    }
}
