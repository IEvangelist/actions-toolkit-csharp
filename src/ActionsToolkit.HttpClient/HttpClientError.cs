// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace ActionsToolkit.HttpClient;

/// <summary>
/// Represents an HTTP client error that captures the originating HTTP
/// <see cref="StatusCode"/>. Mirrors the upstream <c>HttpClientError</c>
/// from <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/index.ts">@actions/http-client</see>.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1710:Identifiers should have correct suffix",
    Justification = "Type name preserved for upstream @actions/http-client parity.")]
public sealed class HttpClientError : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    public HttpClientError(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code returned by the server, as an integer.</param>
    public HttpClientError(string message, int statusCode)
        : this(message, (HttpStatusCode)statusCode)
    {
    }

    /// <summary>
    /// Gets the HTTP status code returned by the server.
    /// </summary>
    public HttpStatusCode StatusCode { get; }
}
