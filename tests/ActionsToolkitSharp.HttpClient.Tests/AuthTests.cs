// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/__tests__/auth.test.ts"/>.
/// Network-bound tests are marked <c>[Trait("Category", "RequiresEnvVar")]</c> and
/// require live access to <c>postman-echo.com</c>; pure-unit equivalents using
/// <see cref="TestHttpMessageHandler"/> exercise the same handler logic offline.
/// </summary>
[Trait("Category", "RequiresEnvVar")]
public class AuthTests
{
    public AuthTests()
    {
        Environment.SetEnvironmentVariable("no_proxy", null);
        Environment.SetEnvironmentVariable("http_proxy", null);
        Environment.SetEnvironmentVariable("https_proxy", null);
    }

    [Fact(DisplayName = "does basic http get request with basic auth")]
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

    [Fact(DisplayName = "does basic http get request with bearer auth")]
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

    [Fact(DisplayName = "does basic http get request with pat token auth")]
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

