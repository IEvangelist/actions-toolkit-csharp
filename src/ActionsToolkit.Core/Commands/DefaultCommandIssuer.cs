// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Commands;

/// <inheritdoc cref="ICommandIssuer" />
internal sealed class DefaultCommandIssuer(IConsole console) : ICommandIssuer
{
    /// <inheritdoc />
    public void Issue(string commandName, string? message = default)
    {
        IssueCommand(commandName, null, message);
    }

    /// <inheritdoc />
    public void IssueCommand(
        string commandName,
        IReadOnlyDictionary<string, string>? properties = default,
        string? message = default)
    {
        var cmd = new Command(
            commandName, message, properties);

        if (cmd is not { Conventional: true })
        {
            console.WriteLine("Issuing unconventional command.");
        }

        var commandMessage = cmd.ToString();
        console.WriteLine(commandMessage);
    }
}
