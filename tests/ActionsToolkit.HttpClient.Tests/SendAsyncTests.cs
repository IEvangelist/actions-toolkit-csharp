// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using NetClient = System.Net.Http.HttpClient;

namespace ActionsToolkit.HttpClient.Tests;

/// <summary>
/// Tests for <see cref="IHttpClient.SendAsync(HttpRequestMessage, System.Threading.CancellationToken)"/>,
/// the .NET equivalent of the upstream <c>requestRaw</c> /
/// <c>requestRawWithCallback</c> APIs.
/// </summary>
public class SendAsyncTests
{
    [Fact(DisplayName = "SendAsync delegates to underlying HttpClient")]
    public async Task SendAsyncDelegatesToUnderlyingHttpClient()
    {
        var test = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/raw");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(test.Requests);
    }

    [Fact(DisplayName = "SendAsync applies credential handler before sending")]
    public async Task SendAsyncAppliesCredentialHandlerBeforeSending()
    {
        var test = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(
            net,
            new BearerCredentialHandler("xyz"));

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/raw");

        await client.SendAsync(request);

        var auth = test.Requests[0].Headers.Authorization!;
        Assert.Equal("Bearer", auth.Scheme);
        Assert.Equal("xyz", auth.Parameter);
    }
}
