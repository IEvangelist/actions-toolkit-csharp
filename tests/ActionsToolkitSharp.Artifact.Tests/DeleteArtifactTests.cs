// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/delete-artifact.test.ts</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class DeleteArtifactTests : IDisposable
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
            () => client.DeleteArtifactAsync("name").AsTask());
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
            () => client.DeleteArtifactAsync("missing").AsTask());
    }

    [Fact(DisplayName = "Issues a DeleteArtifact Twirp request and returns the deleted id")]
    public async Task DeletesAndReturnsId()
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
                    DatabaseId = 99,
                    Name = "name",
                    Size = 100,
                },
            ],
        };
        service.OnDelete = _ => new Internal.Twirp.DeleteArtifactResponse { Ok = true, ArtifactId = 99 };

        var response = await client.DeleteArtifactAsync("name");

        Assert.Equal(99, response.Id);
        Assert.Single(service.DeleteRequests);
    }

    [Fact(DisplayName = "Throws InvalidArtifactResponseException when DeleteArtifact returns ok=false")]
    public async Task ThrowsWhenDeleteNotOk()
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
                    Name = "name",
                    Size = 100,
                },
            ],
        };
        service.OnDelete = _ => new Internal.Twirp.DeleteArtifactResponse { Ok = false, ArtifactId = 0 };

        await Assert.ThrowsAsync<InvalidArtifactResponseException>(
            () => client.DeleteArtifactAsync("name").AsTask());
    }

    [Fact(DisplayName = "FindBy routes through the public REST API delete")]
    public async Task FindByRoutesThroughRest()
    {
        var client = BuildClient(out var service, out var publicApi);
        var findBy = new FindBy("token", 99, "owner", "repo");
        publicApi.OnList = (_, _) =>
        [
            new Artifact("name", 7, 100, DateTimeOffset.UtcNow),
        ];
        publicApi.OnDelete = (_, _) => { /* no-op */ };

        var response = await client.DeleteArtifactAsync("name", new DeleteArtifactOptions { FindBy = findBy });

        Assert.Equal(7, response.Id);
        Assert.Empty(service.ListRequests);
        Assert.Single(publicApi.DeleteInvocations);
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
