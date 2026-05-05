// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient;

/// <summary>
/// Represents the options for an HTTP request.
/// </summary>
/// <param name="Headers"></param>
/// <param name="SocketTimeout"></param>
/// <param name="IgnoreSslError"></param>
/// <param name="AllowRedirects"></param>
/// <param name="AllowRedirectDowngrade"></param>
/// <param name="MaxRedirects"></param>
/// <param name="MaxSockets"></param>
/// <param name="KeepAlive"></param>
/// <param name="DeserializeDates"></param>
/// <param name="AllowRetries"></param>
/// <param name="MaxRetries"></param>
public sealed record class RequestOptions(
    HttpRequestHeaders? Headers,
    int? SocketTimeout,
    bool? IgnoreSslError,
    bool? AllowRedirects,
    bool? AllowRedirectDowngrade,
    int? MaxRedirects,
    int? MaxSockets,
    bool? KeepAlive,
    bool? DeserializeDates,
    // Allows retries only on Read operations (since writes may not be idempotent)
    bool? AllowRetries,
    int? MaxRetries);
