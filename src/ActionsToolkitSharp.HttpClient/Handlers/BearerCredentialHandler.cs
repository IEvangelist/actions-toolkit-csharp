// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Handlers;

internal sealed class BearerCredentialHandler(string token) : IRequestHandler
{
    Dictionary<string, IEnumerable<string>> IRequestHandler.PrepareRequestHeaders(
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
