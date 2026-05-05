// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using NetClient = System.Net.Http.HttpClient;

namespace ActionsToolkitSharp.HttpClient.Tests;

/// <summary>
/// Offline unit tests for the public credential handlers, mirroring the
/// upstream <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/__tests__/auth.test.ts">auth.test.ts</see>
/// suite without requiring a network round-trip. These run in CI alongside
/// the network-bound <see cref="AuthTests"/> class. Each test exercises
/// <see cref="IHttpClient.SendAsync(HttpRequestMessage, CancellationToken)"/>
/// so the handler's <c>Authorization</c> header projection can be inspected
/// without engaging the JSON deserialization pipeline.
/// </summary>
public class AuthHandlersTests
{
    [Fact(DisplayName = "BasicCredentialHandler injects expected Authorization header")]
    public async Task BasicCredentialHandlerInjectsExpectedAuthorizationHeader()
    {
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using var net = new NetClient(handler);

        using IHttpClient client = new DefaultHttpClient(
            net,
            new BasicCredentialHandler("johndoe", "password"));

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/get");

        await client.SendAsync(request);

        var auth = handler.Requests[0].Headers.Authorization!;
        Assert.Equal("Basic", auth.Scheme);
        Assert.Equal("johndoe:password", auth.Parameter!.FromBase64());
    }

    [Fact(DisplayName = "BearerCredentialHandler injects expected Authorization header")]
    public async Task BearerCredentialHandlerInjectsExpectedAuthorizationHeader()
    {
        const string token = "scbfb44vxzku5l4xgc3qfazn3lpk4awflfryc76esaiq7aypcbhs";

        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using var net = new NetClient(handler);

        using IHttpClient client = new DefaultHttpClient(
            net,
            new BearerCredentialHandler(token));

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/get");

        await client.SendAsync(request);

        var auth = handler.Requests[0].Headers.Authorization!;
        Assert.Equal("Bearer", auth.Scheme);
        Assert.Equal(token, auth.Parameter);
    }

    [Fact(DisplayName = "PersonalAccessTokenCredentialHandler injects expected Authorization header")]
    public async Task PersonalAccessTokenCredentialHandlerInjectsExpectedAuthorizationHeader()
    {
        const string pat = "scbfb44vxzku5l4xgc3qfazn3lpk4awflfryc76esaiq7aypcbhs";

        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        using var net = new NetClient(handler);

        using IHttpClient client = new DefaultHttpClient(
            net,
            new PersonalAccessTokenCredentialHandler(pat));

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/get");

        await client.SendAsync(request);

        var auth = handler.Requests[0].Headers.Authorization!;
        Assert.Equal("Basic", auth.Scheme);
        Assert.Equal($"PAT:{pat}", auth.Parameter!.FromBase64());
    }
}
