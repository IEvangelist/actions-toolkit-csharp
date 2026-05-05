// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient;

internal static class ClientNames
{
    /// <summary><c>"Basic"</c> client names corresponding to username and password.</summary>
    internal const string Basic = nameof(Basic);

    /// <summary><c>"Pat"</c> client names corresponding to personal access token (PAT)</summary>
    internal const string Pat = nameof(Pat);

    /// <summary><c>"Bearer"</c> client names corresponding to token.</summary>
    internal const string Bearer = nameof(Bearer);
}
