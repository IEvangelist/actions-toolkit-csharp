// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.ToolCache;

/// <summary>
/// Thrown when a tool-cache HTTP request returns a non-success status code.
/// Mirrors the upstream <c>HTTPError</c> class exposed by
/// <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/src/tool-cache.ts">
/// <c>@actions/tool-cache/tool-cache.ts</c></a>.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1710:Identifiers should have correct suffix",
    Justification = "Mirrors upstream HTTPError class name from @actions/tool-cache for cross-language traceability.")]
[SuppressMessage(
    "Design",
    "CA1032:Implement standard exception constructors",
    Justification = "Mirrors the upstream HTTPError contract which is shaped around the status code.")]
public sealed class HttpError : Exception
{
    /// <summary>
    /// Initializes a new <see cref="HttpError"/> with the given status code.
    /// </summary>
    public HttpError(HttpStatusCode? httpStatusCode)
        : base($"Unexpected HTTP response: {(httpStatusCode is { } c ? ((int)c).ToString(CultureInfo.InvariantCulture) : "<none>")}")
    {
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>
    /// The status code returned by the underlying HTTP response, when
    /// available.
    /// </summary>
    public HttpStatusCode? HttpStatusCode { get; }
}
