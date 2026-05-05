// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Extensions;

internal static class ObjectExtensions
{
    /// <summary>
    /// Converts the given <paramref name="value"/> to a self-identifying dictionary,
    /// where the key name is the property name with itself as a value.
    /// </summary>
    /// <typeparam name="T">The generic-type parameter of the given <paramref name="value"/>.</typeparam>
    /// <param name="value">The <paramref name="value"/> in context.</param>
    /// <param name="paramName">The name of the parameter with which argument corresponds. If you omit this parameter,
    /// the name of argument is used.</param>
    /// <returns>A new dictionary, with a single key-value pair.</returns>
    internal static Dictionary<string, string> ToCommandProperties<T>(
        this T value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        return new()
        {
            [paramName!] = value?.ToString() ?? string.Empty
        };
    }
}
