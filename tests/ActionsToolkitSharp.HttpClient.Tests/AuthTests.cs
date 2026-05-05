// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Tests;

[Trait("Category", "RequiresEnvVar")]
public class AuthTests
{
    public AuthTests()
    {
        Environment.SetEnvironmentVariable("no_proxy", null);
        Environment.SetEnvironmentVariable("http_proxy", null);
        Environment.SetEnvironmentVariable("https_proxy", null);
    }

    [Fact]
    public async Task HttpGetRequestWithBasicAuthCorrectlyDeserializesTypedResponse()
    {
        using var client = new ServiceCollection()
            .AddHttpClientServices()
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateBasicClient("johndoe", "password");

        var response = await client.GetAsync(
            "https://postman-echo.com/get",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        var auth = response.Result.Headers["authorization"];
        var creds = auth["Basic ".Length..].FromBase64();
        Assert.Equal("johndoe:password", creds);
    }

    [Fact]
    public async Task HttpGetRequestWithBearerAuthCorrectlyDeserializesTypedResponse()
    {
        var token = "scbfb44vxzku5l4xgc3qfazn3lpk4awflfryc76esaiq7aypcbhs";

        using var client = new ServiceCollection()
            .AddHttpClientServices()
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreateBearerTokenClient(token);

        var response = await client.GetAsync(
            "https://postman-echo.com/get",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        var auth = response.Result.Headers["authorization"];
        Assert.Equal($"Bearer {token}", auth);
    }

    [Fact]
    public async Task HttpGetRequestWithPatAuthCorrectlyDeserializesTypedResponse()
    {
        var pat = "scbfb44vxzku5l4xgc3qfazn3lpk4awflfryc76esaiq7aypcbhs";

        using var client = new ServiceCollection()
            .AddHttpClientServices()
            .BuildServiceProvider()
            .GetRequiredService<IHttpCredentialClientFactory>()
            .CreatePersonalAccessTokenClient(pat);

        var response = await client.GetAsync(
            "https://postman-echo.com/get",
            SourceGenerationContext.Default.PostmanEchoGetResponse);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Result);
        var auth = response.Result.Headers["authorization"];
        var creds = auth["Basic ".Length..].FromBase64();
        Assert.Equal($"PAT:{pat}", creds);
    }
}
