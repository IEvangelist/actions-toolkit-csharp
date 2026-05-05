// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient.Extensions;

internal static class HttpMethodExtensions
{
    internal static bool IsRetriableMethod(this HttpMethod method)
    {
        return (method.Method[0] | 0x20) switch
        {
            // Retry HTTP methods:
            //   OPTIONS
            //   GET
            //   DELETE
            //   HEAD
            'o' or 'g' or 'd' or 'h' => true,

            _ => false
        };
    }
}