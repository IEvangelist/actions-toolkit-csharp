// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Tests;

/// <summary>
/// Unit tests for the <see cref="HttpCodes"/>, <see cref="Headers"/>, and
/// <see cref="MediaTypes"/> constant types. Mirrors the upstream enums in
/// <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/index.ts">@actions/http-client</see>.
/// </summary>
public class ConstantsTests
{
    [Fact(DisplayName = "HttpCodes exposes upstream-aligned status code constants")]
    public void HttpCodesExposesUpstreamAlignedStatusCodeConstants()
    {
        Assert.Equal(200, HttpCodes.OK);
        Assert.Equal(301, HttpCodes.MovedPermanently);
        Assert.Equal(302, HttpCodes.ResourceMoved);
        Assert.Equal(303, HttpCodes.SeeOther);
        Assert.Equal(307, HttpCodes.TemporaryRedirect);
        Assert.Equal(308, HttpCodes.PermanentRedirect);
        Assert.Equal(401, HttpCodes.Unauthorized);
        Assert.Equal(404, HttpCodes.NotFound);
        Assert.Equal(429, HttpCodes.TooManyRequests);
        Assert.Equal(500, HttpCodes.InternalServerError);
        Assert.Equal(502, HttpCodes.BadGateway);
        Assert.Equal(503, HttpCodes.ServiceUnavailable);
        Assert.Equal(504, HttpCodes.GatewayTimeout);
    }

    [Fact(DisplayName = "Headers exposes upstream-aligned header name constants")]
    public void HeadersExposesUpstreamAlignedHeaderNameConstants()
    {
        Assert.Equal("accept", Headers.Accept);
        Assert.Equal("content-type", Headers.ContentType);
    }

    [Fact(DisplayName = "MediaTypes exposes upstream-aligned media type constants")]
    public void MediaTypesExposesUpstreamAlignedMediaTypeConstants()
    {
        Assert.Equal("application/json", MediaTypes.ApplicationJson);
    }
}
