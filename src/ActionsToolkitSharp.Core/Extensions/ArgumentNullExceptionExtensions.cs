// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Extensions;

internal static class ArgumentNullExceptionExtensions
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is <see langword="null" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which argument corresponds. If you omit this parameter,
    /// the name of argument is used.</param>
    /// <returns>The value of the argument if it is not <see langword="null" />.</returns>
    internal static T ThrowIfNull<T>(
        this T? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);

        return argument;
    }
}
