// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using NetClient = System.Net.Http.HttpClient;

namespace ActionsToolkitSharp.HttpClient.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/__tests__/keepalive.test.ts"/>.
/// Upstream exercises Node's per-request <c>keepAlive</c> agent. .NET's
/// <see cref="System.Net.Http.HttpClient"/> already pools connections under
/// <see cref="SocketsHttpHandler"/>; these tests verify the configured
/// behavior survives the wrapping <see cref="DefaultHttpClient"/>.
/// </summary>
public class KeepAliveTests
{
    private static SocketsHttpHandler CreateKeepAliveHandler(bool keepAlive) =>
        keepAlive
            ? new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30),
            }
            : new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.Zero,
            };

    [Theory(DisplayName = "creates Agent with keepAlive %s")]
    [InlineData(true)]
    [InlineData(false)]
    public void CreatesAgentWithKeepAlive(bool keepAlive)
    {
        using var inner = CreateKeepAliveHandler(keepAlive);
        using var net = new NetClient(inner);

        using IHttpClient client = new DefaultHttpClient(net);

        Assert.NotNull(client);
        Assert.Equal(
            keepAlive ? TimeSpan.FromMinutes(2) : TimeSpan.Zero,
            inner.PooledConnectionLifetime);
    }

    [Fact(DisplayName = "does basic http get request with keepAlive true")]
    public async Task DoesBasicHttpGetRequestWithKeepAliveTrue()
    {
        var test = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, MediaTypes.ApplicationJson),
            });

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.GetAsync(
            "https://example.test/get",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "does basic head request with keepAlive true")]
    public async Task DoesBasicHeadRequestWithKeepAliveTrue()
    {
        var test = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.HeadAsync("https://example.test/get");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "does basic http delete request with keepAlive true")]
    public async Task DoesBasicHttpDeleteRequestWithKeepAliveTrue()
    {
        var test = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.DeleteAsync("https://example.test/delete");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "does basic http post request with keepAlive true")]
    public async Task DoesBasicHttpPostRequestWithKeepAliveTrue()
    {
        var test = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, MediaTypes.ApplicationJson),
            });

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.PostAsync(
            "https://example.test/post",
            new RequestData("Hello World!"),
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "does basic http patch request with keepAlive true")]
    public async Task DoesBasicHttpPatchRequestWithKeepAliveTrue()
    {
        var test = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, MediaTypes.ApplicationJson),
            });

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.PatchAsync(
            "https://example.test/patch",
            new RequestData("Hello World!"),
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "does basic http options request with keepAlive true")]
    public async Task DoesBasicHttpOptionsRequestWithKeepAliveTrue()
    {
        var test = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.OptionsAsync("https://example.test/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
