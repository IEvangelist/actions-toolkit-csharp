// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Extensions;

internal static class StringExtensions
{
    internal static string ToBase64(this string value)
    {
        var inArray = Encoding.UTF8.GetBytes(value);

        return Convert.ToBase64String(inArray);
    }

    internal static string FromBase64(this string value)
    {
        var bytes = Convert.FromBase64String(value);

        return Encoding.UTF8.GetString(bytes);
    }
}
