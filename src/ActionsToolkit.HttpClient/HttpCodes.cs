// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient;

/// <summary>
/// Provides commonly used HTTP status codes as constants. Mirrors the upstream
/// <c>HttpCodes</c> enum from
/// <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/index.ts">@actions/http-client</see>
/// for grep-from-upstream traceability. For richer typing, prefer
/// <see cref="System.Net.HttpStatusCode"/>.
/// </summary>
public static class HttpCodes
{
    /// <summary>HTTP <c>200 OK</c>.</summary>
    public const int OK = 200;

    /// <summary>HTTP <c>300 Multiple Choices</c>.</summary>
    public const int MultipleChoices = 300;

    /// <summary>HTTP <c>301 Moved Permanently</c>.</summary>
    public const int MovedPermanently = 301;

    /// <summary>HTTP <c>302 Found</c> (resource moved).</summary>
    public const int ResourceMoved = 302;

    /// <summary>HTTP <c>303 See Other</c>.</summary>
    public const int SeeOther = 303;

    /// <summary>HTTP <c>304 Not Modified</c>.</summary>
    public const int NotModified = 304;

    /// <summary>HTTP <c>305 Use Proxy</c>.</summary>
    public const int UseProxy = 305;

    /// <summary>HTTP <c>306 Switch Proxy</c>.</summary>
    public const int SwitchProxy = 306;

    /// <summary>HTTP <c>307 Temporary Redirect</c>.</summary>
    public const int TemporaryRedirect = 307;

    /// <summary>HTTP <c>308 Permanent Redirect</c>.</summary>
    public const int PermanentRedirect = 308;

    /// <summary>HTTP <c>400 Bad Request</c>.</summary>
    public const int BadRequest = 400;

    /// <summary>HTTP <c>401 Unauthorized</c>.</summary>
    public const int Unauthorized = 401;

    /// <summary>HTTP <c>402 Payment Required</c>.</summary>
    public const int PaymentRequired = 402;

    /// <summary>HTTP <c>403 Forbidden</c>.</summary>
    public const int Forbidden = 403;

    /// <summary>HTTP <c>404 Not Found</c>.</summary>
    public const int NotFound = 404;

    /// <summary>HTTP <c>405 Method Not Allowed</c>.</summary>
    public const int MethodNotAllowed = 405;

    /// <summary>HTTP <c>406 Not Acceptable</c>.</summary>
    public const int NotAcceptable = 406;

    /// <summary>HTTP <c>407 Proxy Authentication Required</c>.</summary>
    public const int ProxyAuthenticationRequired = 407;

    /// <summary>HTTP <c>408 Request Timeout</c>.</summary>
    public const int RequestTimeout = 408;

    /// <summary>HTTP <c>409 Conflict</c>.</summary>
    public const int Conflict = 409;

    /// <summary>HTTP <c>410 Gone</c>.</summary>
    public const int Gone = 410;

    /// <summary>HTTP <c>429 Too Many Requests</c>.</summary>
    public const int TooManyRequests = 429;

    /// <summary>HTTP <c>500 Internal Server Error</c>.</summary>
    public const int InternalServerError = 500;

    /// <summary>HTTP <c>501 Not Implemented</c>.</summary>
    public const int NotImplemented = 501;

    /// <summary>HTTP <c>502 Bad Gateway</c>.</summary>
    public const int BadGateway = 502;

    /// <summary>HTTP <c>503 Service Unavailable</c>.</summary>
    public const int ServiceUnavailable = 503;

    /// <summary>HTTP <c>504 Gateway Timeout</c>.</summary>
    public const int GatewayTimeout = 504;
}
