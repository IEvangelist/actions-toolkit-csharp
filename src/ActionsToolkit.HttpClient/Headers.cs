// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient;

/// <summary>
/// Common HTTP header names. Mirrors the upstream <c>Headers</c> enum from
/// <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/index.ts">@actions/http-client</see>.
/// </summary>
public static class Headers
{
    /// <summary>The <c>accept</c> HTTP header name.</summary>
    public const string Accept = "accept";

    /// <summary>The <c>content-type</c> HTTP header name.</summary>
    public const string ContentType = "content-type";
}
