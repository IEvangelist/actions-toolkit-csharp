// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/list-artifacts.test.ts</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class ListArtifactsTests : IDisposable
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
            () => client.ListArtifactsAsync().AsTask());
    }

    [Fact(DisplayName = "Lists artifacts via the in-run Twirp service")]
    public async Task ListsViaTwirp()
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
                    DatabaseId = 1,
                    Name = "a",
                    Size = 100,
                    CreatedAt = DateTimeOffset.UtcNow,
                },
                new MonolithArtifact
                {
                    WorkflowRunBackendId = "run-1",
                    WorkflowJobRunBackendId = "job-1",
                    DatabaseId = 2,
                    Name = "b",
                    Size = 200,
                },
            ],
        };

        var response = await client.ListArtifactsAsync();

        Assert.Equal(2, response.Artifacts.Count);
        Assert.Equal("a", response.Artifacts[0].Name);
        Assert.Equal(2, response.Artifacts[1].Id);
    }

    [Fact(DisplayName = "Latest=true filters duplicate names down to the highest id")]
    public async Task LatestFiltersDuplicates()
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
                    DatabaseId = 1,
                    Name = "duplicate",
                    Size = 10,
                },
                new MonolithArtifact
                {
                    WorkflowRunBackendId = "run-1",
                    WorkflowJobRunBackendId = "job-1",
                    DatabaseId = 5,
                    Name = "duplicate",
                    Size = 20,
                },
            ],
        };

        var response = await client.ListArtifactsAsync(new ListArtifactsOptions { Latest = true });

        var artifact = Assert.Single(response.Artifacts);
        Assert.Equal(5, artifact.Id);
    }

    [Fact(DisplayName = "FindBy routes through the public REST API")]
    public async Task FindByRoutesThroughRest()
    {
        var client = BuildClient(out var service, out var publicApi);
        var findBy = new FindBy("token", 99, "owner", "repo");
        publicApi.OnList = (_, _) =>
        [
            new Artifact("a", 1, 100, DateTimeOffset.UtcNow),
        ];

        var response = await client.ListArtifactsAsync(new ListArtifactsOptions { FindBy = findBy });

        Assert.Empty(service.ListRequests);
        Assert.Single(publicApi.ListInvocations);
        Assert.Equal("a", Assert.Single(response.Artifacts).Name);
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
