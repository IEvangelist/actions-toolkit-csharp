// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Handlers;

internal sealed class PersonalAccessTokenHandler(string pat) : IRequestHandler
{
    Dictionary<string, IEnumerable<string>> IRequestHandler.PrepareRequestHeaders(
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
