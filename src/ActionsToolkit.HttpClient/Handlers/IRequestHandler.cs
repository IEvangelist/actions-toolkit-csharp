// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient.Handlers;

/// <summary>
/// Represents a request handler that can mutate or augment request headers
/// (for example, to inject an <c>Authorization</c> header). Mirrors the upstream
/// <c>RequestHandler</c> interface from
/// <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/interfaces.ts">@actions/http-client</see>.
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// Mutates the supplied <paramref name="headers"/> dictionary, adding any
    /// authentication or custom headers required by this handler, and returns it.
    /// </summary>
    /// <param name="headers">The mutable header dictionary to modify.</param>
    /// <returns>The mutated <paramref name="headers"/> dictionary.</returns>
    Dictionary<string, IEnumerable<string>> PrepareRequestHeaders(Dictionary<string, IEnumerable<string>> headers);
}
