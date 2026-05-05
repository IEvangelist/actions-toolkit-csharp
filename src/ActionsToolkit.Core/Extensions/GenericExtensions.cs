// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Extensions;

internal static class GenericExtensions
{
    /// <summary>
    /// Converts the specified object as:
    /// <list type="bullet">
    /// <item>An empty string, when <see langword="null" />.</item>
    /// <item>The value of the string, when <paramref name="value"/> is a string type.</item>
    /// <item>A JSON string, when the <paramref name="value"/> is an object.</item>
    /// </list>
    /// </summary>
    /// <typeparam name="T">The generic-type parameter of the given <paramref name="value"/>.</typeparam>
    /// <param name="value">The <paramref name="value"/> in context.</param>
    /// <param name="typeInfo">The JSON type info, used to serialize the type.</param>
    /// <returns>The string representation of the <paramref name="value"/>.</returns>
    internal static string ToCommandValue<T>(this T? value, JsonTypeInfo<T>? typeInfo = null)
    {
        return IsAnonymousType(typeof(T))
            ? throw new ArgumentException("Generic type T, cannot be anonymous type!")
            : value switch
            {
                null => string.Empty,
                string @string => @string,
                bool @bool => @bool ? "true" : "false",
                _ when typeInfo is null => value?.ToString() ?? string.Empty,
                _ => JsonSerializer.Serialize(value, typeInfo)
            };
    }

    // From: https://stackoverflow.com/a/1650965/2410379
    internal static bool IsAnonymousType(this Type type)
    {
        var typeName = type.Name;

        return typeName.Length switch
        {
            < 3 => false,
            _ => typeName[0] is '<'
            && typeName[1] is '>'
            && typeName.IndexOf("AnonymousType", StringComparison.Ordinal) > 0
        };
    }
}
