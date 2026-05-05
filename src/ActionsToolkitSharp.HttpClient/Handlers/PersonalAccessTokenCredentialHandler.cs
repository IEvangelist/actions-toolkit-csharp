// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Handlers;

/// <summary>
/// An <see cref="IRequestHandler"/> that injects an HTTP Basic
/// <c>Authorization</c> header derived from a personal access token (PAT),
/// using the literal username <c>"PAT"</c>. Mirrors the upstream
/// <c>PersonalAccessTokenCredentialHandler</c> from
/// <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/auth.ts">@actions/http-client</see>.
/// </summary>
/// <param name="pat">The personal access token used to authenticate requests.</param>
public sealed class PersonalAccessTokenCredentialHandler(string pat) : IRequestHandler
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
                $"PAT:{pat}".ToBase64()
            )
            .ToString()
        ];

        return headers;
    }
}
