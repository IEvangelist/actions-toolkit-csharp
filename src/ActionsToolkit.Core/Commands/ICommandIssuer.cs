// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Commands;

/// <summary>
/// The utility used to issue commands.
/// </summary>
internal interface ICommandIssuer
{
    /// <summary>
    /// Issue a formal command, given its <paramref name="commandName"/>, <paramref name="properties"/> and <paramref name="message"/>.
    /// The following format is adhered to:
    /// <c>::name key=value,key=value::message</c>
    /// Consider the following examples:
    /// <example>
    /// <list type="bullet">
    /// <item><c>::warning::This is the message</c></item>
    /// <item><c>::set-env name=MY_VAR::some value</c></item>
    /// </list>
    /// </example>
    /// </summary>
    /// <param name="commandName">Formal command name as defined in <see cref="CommandNames" /></param>
    /// <param name="properties">Properties to issue as part of the command, written as key-value pairs.</param>
    /// <param name="message">An arbitrary message value</param>
    void IssueCommand(
        string commandName,
        IReadOnlyDictionary<string, string>? properties = default,
        string? message = default);

    /// <summary>
    /// Issue a formal command, given its <paramref name="commandName"/> and <paramref name="message"/>.
    /// </summary>
    /// <param name="commandName">Formal command name as defined in <see cref="CommandNames" /></param>
    /// <param name="message">An arbitrary message value</param>
    void Issue(string commandName, string? message = default);
}
