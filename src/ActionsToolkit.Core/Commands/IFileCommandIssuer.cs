// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Commands;

/// <summary>
/// The utility used to issue file-based commands.
/// </summary>
internal interface IFileCommandIssuer
{
    /// <summary>
    /// Asynchronous I/O that issues a command that corresponds to the
    /// given <paramref name="commandSuffix"/>, with the given <paramref name="message"/> value.
    /// </summary>
    /// <param name="commandSuffix">The command suffix as found in <see cref="EnvironmentVariables.Suffixes"/></param>
    /// <param name="message">An arbitrary message value</param>
    /// <param name="typeInfo">The JSON type info used to serialize.</param>
    /// <returns>A task that represents the asynchronous operation of writing the message to file.</returns>
    ValueTask IssueFileCommandAsync<TValue>(
        string commandSuffix,
        TValue message,
        JsonTypeInfo<TValue>? typeInfo = null);

    /// <summary>
    /// Prepares a key-value message, given the <paramref name="key"/> and <paramref name="value"/>.
    /// </summary>
    /// <param name="key">The key used as the left-operand.</param>
    /// <param name="value">The value used as the right-operand.</param>
    /// <param name="typeInfo">The JSON type info used to serialize.</param>
    /// <returns>
    /// A string representation of the key-value pair, formatted
    /// with the appropriate unique delimiter.
    /// </returns>
    string PrepareKeyValueMessage<TValue>(
        string key,
        TValue value,
        JsonTypeInfo<TValue>? typeInfo = null);
}
