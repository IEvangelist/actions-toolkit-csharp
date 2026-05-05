// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Mirrors upstream
/// <c>actions/toolkit/packages/artifact/__tests__/artifact-http-client.test.ts</c>.
/// In the upstream Node port, retry/timeout/backoff lives inside the
/// <c>internalArtifactTwirpClient</c> helper. In the C# port the same
/// concerns are split: bearer-token/auth-header propagation and resilience
/// (retry/timeouts) are handled by the named <see cref="IHttpClientFactory"/>
/// registration in <see cref="ServiceCollectionExtensions.AddGitHubActionsArtifact"/>
/// (via <c>AddStandardResilienceHandler</c>), while the request shape and
/// response decoding live in <see cref="DefaultArtifactService"/>. These
/// tests exercise the <see cref="DefaultArtifactService"/> surface against
/// scripted responses produced by <see cref="TestHttpMessageHandler"/>.
/// Retry/multi-attempt scenarios are intentionally not mirrored here since
/// the resilience handler is composed at DI time and is exercised via the
/// upstream Microsoft.Extensions.Http.Resilience tests; the parity gap is
/// recorded in the package README.
/// </summary>
public sealed class ArtifactHttpClientTests
{
    private const string SignedUploadUrl = "http://localhost:8080/upload";
    private static readonly Uri s_baseAddress = new("http://localhost:8080/");

    private static readonly CreateArtifactRequest s_createRequest = new()
    {
        WorkflowRunBackendId = "1234",
        WorkflowJobRunBackendId = "5678",
        Name = "artifact",
        Version = 4,
    };

    [Fact(DisplayName = "should successfully create a client")]
    public void ShouldSuccessfullyCreateAClient()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var service = BuildService(handler);

        Assert.NotNull(service);
    }

    [Fact(DisplayName = "should make a request")]
    public async Task ShouldMakeARequest()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent(
                $$"""{"ok": true, "signed_upload_url": "{{SignedUploadUrl}}"}"""),
        });
        var service = BuildService(handler);

        var response = await service.CreateArtifactAsync(s_createRequest, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Ok);
        Assert.Equal(SignedUploadUrl, response.SignedUploadUrl);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.EndsWith(
            "/twirp/github.actions.results.api.v1.ArtifactService/CreateArtifact",
            request.RequestUri!.AbsolutePath,
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "request body is serialized as application/json")]
    public async Task RequestBodyIsApplicationJson()
    {
        HttpRequestMessage? captured = null;
        var handler = new TestHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(
                    $$"""{"ok": true, "signed_upload_url": "{{SignedUploadUrl}}"}"""),
            };
        });
        var service = BuildService(handler);

        await service.CreateArtifactAsync(s_createRequest, CancellationToken.None);

        Assert.NotNull(captured?.Content);
        Assert.Equal(
            "application/json",
            captured!.Content!.Headers.ContentType?.MediaType);
        var body = await captured.Content.ReadAsStringAsync();
        Assert.Contains("\"workflow_run_backend_id\":\"1234\"", body, StringComparison.Ordinal);
        Assert.Contains("\"workflow_job_run_backend_id\":\"5678\"", body, StringComparison.Ordinal);
        Assert.Contains("\"name\":\"artifact\"", body, StringComparison.Ordinal);
        Assert.Contains("\"version\":4", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Authorization header is propagated from the injected NetClient")]
    public async Task AuthorizationHeaderIsPropagated()
    {
        HttpRequestMessage? captured = null;
        var handler = new TestHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(
                    $$"""{"ok": true, "signed_upload_url": "{{SignedUploadUrl}}"}"""),
            };
        });
        var service = BuildService(
            handler,
            client => client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "secret-token"));

        await service.CreateArtifactAsync(s_createRequest, CancellationToken.None);

        Assert.Equal("Bearer", captured?.Headers.Authorization?.Scheme);
        Assert.Equal("secret-token", captured?.Headers.Authorization?.Parameter);
    }

    [Fact(DisplayName = "should fail immediately if there is a non-retryable error (401)")]
    public async Task ShouldFailImmediatelyOnNonRetryableError()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = JsonContent("""{"ok": false}"""),
            ReasonPhrase = "Unauthorized",
        });
        var service = BuildService(handler);

        var ex = await Assert.ThrowsAsync<InvalidArtifactResponseException>(
            () => service.CreateArtifactAsync(s_createRequest, CancellationToken.None));

        Assert.Contains("CreateArtifact", ex.Message, StringComparison.Ordinal);
        Assert.Contains("401", ex.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
    }

    [Fact(DisplayName = "should properly describe a usage error (403)")]
    public async Task ShouldDescribeUsageError()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = JsonContent("""{"msg": "insufficient usage to create artifact"}"""),
            ReasonPhrase = "Forbidden",
        });
        var service = BuildService(handler);

        var ex = await Assert.ThrowsAsync<InvalidArtifactResponseException>(
            () => service.CreateArtifactAsync(s_createRequest, CancellationToken.None));

        Assert.Contains("403", ex.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "should fail if the request fails with 5xx")]
    public async Task ShouldFailOnServerError()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = JsonContent("""{"ok": false}"""),
            ReasonPhrase = "Internal Server Error",
        });
        var service = BuildService(handler);

        var ex = await Assert.ThrowsAsync<InvalidArtifactResponseException>(
            () => service.CreateArtifactAsync(s_createRequest, CancellationToken.None));

        Assert.Contains("500", ex.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "200 response with empty body throws InvalidArtifactResponseException")]
    public async Task EmptyBodyThrows()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent("null"),
        });
        var service = BuildService(handler);

        await Assert.ThrowsAsync<InvalidArtifactResponseException>(
            () => service.CreateArtifactAsync(s_createRequest, CancellationToken.None));
    }

    [Fact(DisplayName = "ListArtifacts POSTs to ListArtifacts and decodes the artifacts array")]
    public async Task ListArtifactsRoundTrip()
    {
        const string ListJson = """
            {
              "artifacts": [
                {
                  "workflow_run_backend_id": "run-1",
                  "workflow_job_run_backend_id": "job-1",
                  "database_id": 42,
                  "name": "artifact",
                  "size": 100
                }
              ]
            }
            """;

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent(ListJson),
        });
        var service = BuildService(handler);

        var response = await service.ListArtifactsAsync(
            new ListArtifactsRequest
            {
                WorkflowRunBackendId = "run-1",
                WorkflowJobRunBackendId = "job-1",
            },
            CancellationToken.None);

        var artifact = Assert.Single(response.Artifacts);
        Assert.Equal(42, artifact.DatabaseId);
        Assert.Equal("artifact", artifact.Name);
        Assert.EndsWith(
            "/twirp/github.actions.results.api.v1.ArtifactService/ListArtifacts",
            handler.Requests[0].RequestUri!.AbsolutePath,
            StringComparison.Ordinal);
    }

    private static DefaultArtifactService BuildService(
        TestHttpMessageHandler handler,
        Action<System.Net.Http.HttpClient>? configure = null)
    {
        var client = new System.Net.Http.HttpClient(handler, disposeHandler: false)
        {
            BaseAddress = s_baseAddress,
        };
        configure?.Invoke(client);
        return new DefaultArtifactService(client);
    }

    private static StringContent JsonContent(string json) =>
        new(json, Encoding.UTF8, "application/json");
}
