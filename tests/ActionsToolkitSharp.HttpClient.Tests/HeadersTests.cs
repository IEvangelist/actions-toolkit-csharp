// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using NetClient = System.Net.Http.HttpClient;

namespace ActionsToolkitSharp.HttpClient.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/__tests__/headers.test.ts"/>.
/// Upstream exercises <c>getJson</c>/<c>postJson</c>/<c>putJson</c>/<c>patchJson</c>;
/// the .NET surface is always JSON, so each upstream <c>it('…')</c> below
/// asserts that consumer-supplied <c>additionalHeaders</c> reach the wire
/// for the corresponding verb. Tests are run offline using
/// <see cref="TestHttpMessageHandler"/>.
/// </summary>
public class HeadersTests
{
    private static (NetClient client, TestHttpMessageHandler handler) CreateClient(string responseJson = "{}")
    {
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypes.ApplicationJson),
            });

        var client = new NetClient(handler);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.ApplicationJson));
        return (client, handler);
    }

    [Fact(DisplayName = "preserves existing headers on getJson")]
    public async Task PreservesExistingHeadersOnGetJson()
    {
        var (net, handler) = CreateClient();
        using var _ = net;

        using IHttpClient httpClient = new DefaultHttpClient(net);

        Dictionary<string, IEnumerable<string>> additionalHeaders = new()
        {
            [Headers.Accept] = ["foo"],
        };

        await httpClient.GetAsync(
            "https://example.test/get",
            SourceGenerationContext.Default.PostmanEchoGetResponse,
            additionalHeaders);

        var sent = handler.Requests[0];
        Assert.Contains("foo", sent.Headers.GetValues("Accept"));
    }

    [Fact(DisplayName = "preserves existing headers on postJson")]
    public async Task PreservesExistingHeadersOnPostJson()
    {
        var (net, handler) = CreateClient();
        using var _ = net;

        using IHttpClient httpClient = new DefaultHttpClient(net);

        Dictionary<string, IEnumerable<string>> additionalHeaders = new()
        {
            [Headers.Accept] = ["foo"],
        };

        await httpClient.PostAsync(
            "https://example.test/post",
            new RequestData("payload"),
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse,
            additionalHeaders);

        var sent = handler.Requests[0];
        Assert.Contains("foo", sent.Headers.GetValues("Accept"));
        Assert.Equal(MediaTypes.ApplicationJson, sent.Content!.Headers.ContentType!.MediaType);
    }

    [Fact(DisplayName = "preserves existing headers on putJson")]
    public async Task PreservesExistingHeadersOnPutJson()
    {
        var (net, handler) = CreateClient();
        using var _ = net;

        using IHttpClient httpClient = new DefaultHttpClient(net);

        Dictionary<string, IEnumerable<string>> additionalHeaders = new()
        {
            [Headers.Accept] = ["foo"],
        };

        await httpClient.PutAsync(
            "https://example.test/put",
            new RequestData("payload"),
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse,
            additionalHeaders);

        var sent = handler.Requests[0];
        Assert.Contains("foo", sent.Headers.GetValues("Accept"));
        Assert.Equal(MediaTypes.ApplicationJson, sent.Content!.Headers.ContentType!.MediaType);
    }

    [Fact(DisplayName = "preserves existing headers on patchJson")]
    public async Task PreservesExistingHeadersOnPatchJson()
    {
        var (net, handler) = CreateClient();
        using var _ = net;

        using IHttpClient httpClient = new DefaultHttpClient(net);

        Dictionary<string, IEnumerable<string>> additionalHeaders = new()
        {
            [Headers.Accept] = ["foo"],
        };

        await httpClient.PatchAsync(
            "https://example.test/patch",
            new RequestData("payload"),
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse,
            additionalHeaders);

        var sent = handler.Requests[0];
        Assert.Contains("foo", sent.Headers.GetValues("Accept"));
        Assert.Equal(MediaTypes.ApplicationJson, sent.Content!.Headers.ContentType!.MediaType);
    }
}
