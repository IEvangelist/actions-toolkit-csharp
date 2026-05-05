// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient;

/// <summary>
/// A strongly-typed response object that includes the HTTP status code, the result, and the response HTTP headers.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="StatusCode">The HTTP status code of the response.</param>
/// <param name="Result">The resulting object instance for the response.</param>
/// <param name="ResponseHttpHeaders">The response HTTP headers.</param>
public sealed record class TypedResponse<TResult>(
    HttpStatusCode StatusCode,
    TResult? Result = default,
    HttpResponseHeaders? ResponseHttpHeaders = null);
