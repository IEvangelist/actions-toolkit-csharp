// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Output;

/// <summary>
/// A console interface that abstracts the interactions of the <see cref="Console"/>.
/// </summary>
public interface IConsole
{
    /// <inheritdoc cref="Exit" />
    void ExitWithCode(int exitCode = 0)
    {
        Exit(Environment.ExitCode = exitCode);
    }

    /// <inheritdoc cref="Console.Write(string?)" />
    void Write(string? message = null)
    {
        Console.Write(message);
    }

    /// <inheritdoc cref="Console.WriteLine(string?)" />
    void WriteLine(string? message = null)
    {
        Console.WriteLine(message);
    }

    /// <inheritdoc cref="TextWriter.Write(string?)" />
    void WriteError(string message)
    {
        Console.Error.Write(message);
    }

    /// <inheritdoc cref="TextWriter.WriteLine(string?)" />
    void WriteErrorLine(string message)
    {
        Console.Error.WriteLine(message);
    }
}
