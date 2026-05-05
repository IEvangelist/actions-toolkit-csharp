// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient.Handlers;

/// <summary>
/// An <see cref="IRequestHandler"/> that injects an HTTP Basic
/// <c>Authorization</c> header derived from the supplied
/// <paramref name="username"/> and <paramref name="password"/>. Mirrors the upstream
/// <c>BasicCredentialHandler</c> from
/// <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/auth.ts">@actions/http-client</see>.
/// </summary>
/// <param name="username">The username portion of the Basic credential.</param>
/// <param name="password">The password portion of the Basic credential.</param>
public sealed class BasicCredentialHandler(string username, string password) : IRequestHandler
{
    /// <inheritdoc />
    public Dictionary<string, IEnumerable<string>> PrepareRequestHeaders(
        Dictionary<string, IEnumerable<string>> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        headers["Authorization"] =
        [
            new AuthenticationHeaderValue(
                "Basic",
                $"{username}:{password}".ToBase64()
            )
            .ToString()
        ];

        return headers;
    }
}
