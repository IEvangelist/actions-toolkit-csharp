// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Exec;

/// <summary>
/// Cross-platform process execution service that mirrors the public surface of
/// the upstream <a href="https://github.com/actions/toolkit/tree/main/packages/exec"><c>@actions/exec</c></a>
/// (3.0.0) Node.js package.
/// </summary>
public interface IExecService
{
    /// <summary>
    /// Executes a command, streaming output to the live console (and any configured
    /// listeners) and returning the resulting exit code.
    /// </summary>
    /// <param name="commandLine">The command to execute. May include additional args
    /// (which must be correctly quoted/escaped).</param>
    /// <param name="args">Optional, additional arguments. Escaping is handled by the runner.</param>
    /// <param name="options">Optional <see cref="ExecOptions"/>.</param>
    /// <param name="cancellationToken">Cancellation token. When triggered, the child
    /// process is killed (entire process tree).</param>
    /// <returns>The child process exit code.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the process exits non-zero (and <see cref="ExecOptions.IgnoreReturnCode"/> is
    /// not set), or when <see cref="ExecOptions.FailOnStdErr"/> is set and the process wrote to
    /// <c>stderr</c>.
    /// </exception>
    ValueTask<int> ExecAsync(
        string commandLine,
        string[]? args = null,
        ExecOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command and captures the full <c>stdout</c> / <c>stderr</c> in the
    /// returned <see cref="ExecOutput"/>. Output is still streamed to the live console
    /// (and any configured listeners) while it accumulates.
    /// </summary>
    /// <param name="commandLine">The command to execute.</param>
    /// <param name="args">Optional, additional arguments.</param>
    /// <param name="options">Optional <see cref="ExecOptions"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ExecOutput"/> with exit code, stdout and stderr.</returns>
    ValueTask<ExecOutput> GetExecOutputAsync(
        string commandLine,
        string[]? args = null,
        ExecOptions? options = null,
        CancellationToken cancellationToken = default);
}
