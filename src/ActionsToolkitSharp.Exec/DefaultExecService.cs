// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Exec;

/// <inheritdoc cref="IExecService"/>
internal sealed class DefaultExecService : IExecService
{
    /// <inheritdoc/>
    public ValueTask<int> ExecAsync(
        string commandLine,
        string[]? args = null,
        ExecOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(commandLine);

        var commandArgs = ArgString.ToArray(commandLine);
        if (commandArgs.Length == 0)
        {
            throw new ArgumentException("Parameter 'commandLine' cannot be null or empty.", nameof(commandLine));
        }

        var toolPath = commandArgs[0];
        // remaining tokens of commandLine come first, then explicit args (mirrors upstream).
        var tail = new List<string>(commandArgs.Length - 1 + (args?.Length ?? 0));
        for (var i = 1; i < commandArgs.Length; i++)
        {
            tail.Add(commandArgs[i]);
        }
        if (args is not null)
        {
            tail.AddRange(args);
        }

        var runner = new ToolRunner(toolPath, [.. tail], options);
        return runner.ExecAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async ValueTask<ExecOutput> GetExecOutputAsync(
        string commandLine,
        string[]? args = null,
        ExecOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(commandLine);

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var originalStdout = options?.Listeners?.Stdout;
        var originalStderr = options?.Listeners?.Stderr;

        var listeners = new ExecListeners
        {
            Stdout = data =>
            {
                stdout.Append(Encoding.UTF8.GetString(data.Span));
                originalStdout?.Invoke(data);
            },
            Stderr = data =>
            {
                stderr.Append(Encoding.UTF8.GetString(data.Span));
                originalStderr?.Invoke(data);
            },
            Stdline = options?.Listeners?.Stdline,
            Errline = options?.Listeners?.Errline,
            Debug = options?.Listeners?.Debug,
        };

        var mergedOptions = options is null
            ? new ExecOptions { Listeners = listeners }
            : new ExecOptions
            {
                Cwd = options.Cwd,
                Env = options.Env,
                Input = options.Input,
                Silent = options.Silent,
                FailOnStdErr = options.FailOnStdErr,
                IgnoreReturnCode = options.IgnoreReturnCode,
                OutStream = options.OutStream,
                ErrStream = options.ErrStream,
                WindowsVerbatimArguments = options.WindowsVerbatimArguments,
                Delay = options.Delay,
                Listeners = listeners,
            };

        var exitCode = await ExecAsync(commandLine, args, mergedOptions, cancellationToken).ConfigureAwait(false);

        return new ExecOutput(exitCode, stdout.ToString(), stderr.ToString());
    }
}
