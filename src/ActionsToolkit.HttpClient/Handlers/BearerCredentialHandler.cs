// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient.Handlers;

/// <summary>
/// An <see cref="IRequestHandler"/> that injects an HTTP Bearer
/// <c>Authorization</c> header derived from the supplied
/// <paramref name="token"/>. Mirrors the upstream <c>BearerCredentialHandler</c>
/// from <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/auth.ts">@actions/http-client</see>.
/// </summary>
/// <param name="token">The bearer token used to authenticate requests.</param>
public sealed class BearerCredentialHandler(string token) : IRequestHandler
{
    /// <inheritdoc />
    public Dictionary<string, IEnumerable<string>> PrepareRequestHeaders(
        Dictionary<string, IEnumerable<string>> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        headers["Authorization"] =
        [
            new AuthenticationHeaderValue("Bearer", token).ToString()
        ];

        return headers;
    }
}
