// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using NetClient = System.Net.Http.HttpClient;

namespace ActionsToolkitSharp.HttpClient.Tests;

/// <summary>
/// Offline counterpart to <see cref="BasicTests"/>. Mirrors a subset of
/// upstream <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/__tests__/basics.test.ts">basics.test.ts</see>
/// cases that don't require live <c>postman-echo.com</c> connectivity.
/// </summary>
public class BasicsOfflineTests
{
    [Fact(DisplayName = "constructs")]
    public void Constructs()
    {
        using var net = new NetClient(new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));

        using IHttpClient client = new DefaultHttpClient(net);

        Assert.NotNull(client);
    }

    [Fact(DisplayName = "gets a json object")]
    public async Task GetsAJsonObject()
    {
        var test = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"data\":{\"Message\":\"hello\"},\"args\":{},\"headers\":{},\"url\":\"https://example.test/get\"}",
                    Encoding.UTF8,
                    MediaTypes.ApplicationJson),
            });

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.GetAsync(
            "https://example.test/get",
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        Assert.Equal("https://example.test/get", response.Result.Url);
        Assert.Equal("hello", response.Result.Data.Message);
    }

    [Fact(DisplayName = "getting a non existent json object returns null")]
    public async Task GettingANonExistentJsonObjectReturnsNull()
    {
        var test = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.GetAsync(
            "https://example.test/missing",
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(response.Result);
    }

    [Fact(DisplayName = "posts a json object")]
    public async Task PostsAJsonObject()
    {
        var test = new TestHttpMessageHandler(req =>
        {
            Assert.NotNull(req.Content);
            Assert.Equal(MediaTypes.ApplicationJson, req.Content!.Headers.ContentType!.MediaType);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"data\":{\"Message\":\"foo\"},\"args\":{},\"headers\":{},\"url\":\"https://example.test/post\"}",
                    Encoding.UTF8,
                    MediaTypes.ApplicationJson),
            };
        });

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.PostAsync(
            "https://example.test/post",
            new RequestData("foo"),
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        Assert.Equal("foo", response.Result.Data.Message);
    }

    [Fact(DisplayName = "patch a json object")]
    public async Task PatchAJsonObject()
    {
        var test = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"data\":{\"Message\":\"foo\"},\"args\":{},\"headers\":{},\"url\":\"https://example.test/patch\"}",
                    Encoding.UTF8,
                    MediaTypes.ApplicationJson),
            });

        using var net = new NetClient(test);

        using IHttpClient client = new DefaultHttpClient(net);

        var response = await client.PatchAsync(
            "https://example.test/patch",
            new RequestData("foo"),
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        Assert.Equal("foo", response.Result.Data.Message);
    }
}
