// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/artifact-http-client.test.ts</c>: covers the
/// REST surface (list/download/delete) routed through
/// <see cref="DefaultPublicArtifactsApi"/>. The handler is fed scripted
/// responses; assertions inspect the outgoing requests for correctness.
/// </summary>
public sealed class PublicArtifactsApiTests
{
    private static readonly Uri s_apiBase = new("https://api.github.com/");
    private static readonly FindBy s_findBy = new("token", 99, "owner", "repo");

    [Fact(DisplayName = "ListAsync issues GET to actions/runs/{runId}/artifacts")]
    public async Task ListIssuesGet()
    {
        var json = JsonSerializer.Serialize(
            new PublicArtifactsListResponse
            {
                TotalCount = 1,
                Artifacts =
                [
                    new PublicArtifactItem
                    {
                        Id = 5,
                        Name = "name",
                        SizeInBytes = 100,
                        CreatedAt = DateTimeOffset.UtcNow,
                        Digest = "sha256:abc",
                    },
                ],
            },
            PublicArtifactsJsonContext.Default.PublicArtifactsListResponse);

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
        var api = BuildApi(handler);

        var artifacts = await api.ListAsync(s_findBy, nameFilter: "name", CancellationToken.None);

        var artifact = Assert.Single(artifacts);
        Assert.Equal(5, artifact.Id);
        Assert.Equal("sha256:abc", artifact.Digest);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Contains("repos/owner/repo/actions/runs/99/artifacts", request.RequestUri!.AbsoluteUri, StringComparison.Ordinal);
        Assert.Contains("name=name", request.RequestUri.Query, StringComparison.Ordinal);
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("token", request.Headers.Authorization?.Parameter);
    }

    [Fact(DisplayName = "ListAsync throws InvalidArtifactResponseException on non-success status")]
    public async Task ListThrowsOnFailure()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var api = BuildApi(handler);

        await Assert.ThrowsAsync<InvalidArtifactResponseException>(
            () => api.ListAsync(s_findBy, nameFilter: null, CancellationToken.None));
    }

    [Fact(DisplayName = "GetDownloadRedirectAsync returns the Location header on 302")]
    public async Task DownloadReturnsLocation()
    {
        var location = new Uri("https://blob.example.com/sig");
        var handler = new TestHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Found);
            response.Headers.Location = location;
            return response;
        });
        var api = BuildApi(handler);

        var result = await api.GetDownloadRedirectAsync(s_findBy, 42, CancellationToken.None);

        Assert.Equal(location, result);
        Assert.Contains("actions/artifacts/42/zip", handler.Requests[0].RequestUri!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "GetDownloadRedirectAsync throws when Location header missing")]
    public async Task DownloadThrowsWithoutLocation()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Found));
        var api = BuildApi(handler);

        await Assert.ThrowsAsync<InvalidArtifactResponseException>(
            () => api.GetDownloadRedirectAsync(s_findBy, 42, CancellationToken.None));
    }

    [Fact(DisplayName = "DeleteAsync issues DELETE to actions/artifacts/{id}")]
    public async Task DeleteIssuesDelete()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var api = BuildApi(handler);

        await api.DeleteAsync(s_findBy, 42, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Contains("actions/artifacts/42", request.RequestUri!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "DeleteAsync throws when status is not success")]
    public async Task DeleteThrowsOnFailure()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var api = BuildApi(handler);

        await Assert.ThrowsAsync<InvalidArtifactResponseException>(
            () => api.DeleteAsync(s_findBy, 42, CancellationToken.None));
    }

    private static DefaultPublicArtifactsApi BuildApi(TestHttpMessageHandler handler)
    {
        var factory = new ApiHttpClientFactory(handler);
        return new DefaultPublicArtifactsApi(factory);
    }

    private sealed class ApiHttpClientFactory : IHttpClientFactory
    {
        private readonly TestHttpMessageHandler _handler;
        public ApiHttpClientFactory(TestHttpMessageHandler handler) => _handler = handler;

        public System.Net.Http.HttpClient CreateClient(string name) => new(_handler, disposeHandler: false)
        {
            BaseAddress = s_apiBase,
        };
    }
}
