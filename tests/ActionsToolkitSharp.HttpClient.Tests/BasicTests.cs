// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Tests;

public class BasicTests
{
    public BasicTests()
    {
        Environment.SetEnvironmentVariable("no_proxy", null);
        Environment.SetEnvironmentVariable("http_proxy", null);
        Environment.SetEnvironmentVariable("https_proxy", null);
    }

    [Theory(Skip = "postman-echo.com redirects HTTP to HTTPS")]
    [InlineData("http://postman-echo.com/get", "functional-test-agent")]
    [InlineData("http://postman-echo.com/get", "test-agent")]
    public async Task DoesBasicGetRequestWithUserAgent(string requestUri, string userAgent)
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.GetAsync(
            requestUri,
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        Assert.Equal(requestUri, response.Result.Url);
        Assert.Equal(userAgent, response.Result.Headers["user-agent"]);
    }

    [Fact]
    public async Task DoesBasicHttpGetRequestWithNoUserAgent()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.GetAsync(
            "https://postman-echo.com/get",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        Assert.False(response.Result.Headers.ContainsKey("user-agent"));
    }

    [Fact]
    public async Task DoesBasicHttpGetRequestWithDefaultHeaders()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.GetAsync(
            "https://postman-echo.com/get",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        Assert.Equal("application/json", response.Result.Headers["accept"]);
    }

    [Fact]
    public async Task DoesBasicHttpGetRequestWithMergedHeaders()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.GetAsync(
            $"https://postman-echo.com/redirect-to?url={Uri.EscapeDataString("https://postman-echo.com/get")}",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
    }

    [Fact]
    public async Task DoesBasicGetRequestWithRedirect()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.GetAsync(
            $"https://postman-echo.com/redirect-to?url={Uri.EscapeDataString("https://postman-echo.com/get")}&status_code=303",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
    }

    [Fact]
    public async Task ReturnsNotFoundGetRequestOnRedirect()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.GetAsync(
            $"https://postman-echo.com/redirect-to?url={Uri.EscapeDataString("https://postman-echo.com/status/404")}&status_code=303",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(response.Result);
    }

    [Fact]
    public async Task DoesNotPassAuthWithDiffHostnameRedirects()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.GetAsync(
            $"https://postman-echo.com/redirect-to?url={Uri.EscapeDataString("https://postman-echo.com/get")}&status_code=303",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        Assert.Equal("application/json", response.Result.Headers["accept"]);
        Assert.False(response.Result.Headers.ContainsKey("authorization"));
        Assert.False(response.Result.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task DoesBasicHeadRequest()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.HeadAsync("http://postman-echo.com/get");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DoesBasicDeleteRequest()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.DeleteAsync("http://postman-echo.com/delete");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "postman-echo.com redirects HTTP to HTTPS")]
    public async Task DoesBasicHttpPostRequest()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var data = new RequestData("Hello World!");

        var response = await client.PostAsync(
            "http://postman-echo.com/post",
            data,
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(data, response.Result!.Data);
    }

    [Fact]
    public async Task DoesBasicHttpPatchRequest()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var data = new RequestData("Hello World!");

        var response = await client.PatchAsync(
            "http://postman-echo.com/patch",
            data,
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(data, response.Result!.Data);
    }

    [Fact]
    public async Task DoesBasicHttpPutRequest()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var data = new RequestData("Hello World!");

        var response = await client.PutAsync(
            "http://postman-echo.com/put",
            data,
            SourceGenerationContext.Default.RequestData,
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(data, response.Result!.Data);
    }

    [Fact(Skip = "postman-echo.com redirects HTTP to HTTPS")]
    public async Task DoesBasicOptionsRequest()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.OptionsAsync("http://postman-echo.com");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReturnsNotFoundGetRequest()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices(userAgent: null)
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateClient();

        var response = await client.GetAsync(
            "http://postman-echo.com/status/404",
            SourceGenerationContext.Default.PostmanEchoResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(response.Result);
    }
}
