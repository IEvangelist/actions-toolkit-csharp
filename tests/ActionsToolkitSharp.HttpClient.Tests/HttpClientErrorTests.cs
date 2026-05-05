// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Tests;

/// <summary>
/// Unit tests for the <see cref="HttpClientError"/> exception type, which
/// mirrors the upstream <c>HttpClientError</c> from
/// <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/index.ts">@actions/http-client</see>.
/// </summary>
public class HttpClientErrorTests
{
    [Fact(DisplayName = "HttpClientError captures status code and message")]
    public void HttpClientErrorCapturesStatusCodeAndMessage()
    {
        var error = new HttpClientError("Something broke", HttpStatusCode.InternalServerError);

        Assert.Equal("Something broke", error.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, error.StatusCode);
    }

    [Fact(DisplayName = "HttpClientError accepts integer status code overload")]
    public void HttpClientErrorAcceptsIntegerStatusCodeOverload()
    {
        var error = new HttpClientError("Not found", HttpCodes.NotFound);

        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
    }
}
