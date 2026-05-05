// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Exec;

/// <summary>
/// Optional callbacks for streaming output and lifecycle events from a process
/// invoked through <see cref="IExecService"/>. Mirrors the upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/exec/src/interfaces.ts"><c>ExecListeners</c></a>
/// interface from <c>@actions/exec</c>.
/// </summary>
public sealed class ExecListeners
{
    /// <summary>
    /// Invoked once for every <c>stdout</c> chunk emitted by the child process.
    /// The byte buffer is the raw UTF-8 chunk as received.
    /// </summary>
    public Action<ReadOnlyMemory<byte>>? Stdout { get; init; }

    /// <summary>
    /// Invoked once for every <c>stderr</c> chunk emitted by the child process.
    /// The byte buffer is the raw UTF-8 chunk as received.
    /// </summary>
    public Action<ReadOnlyMemory<byte>>? Stderr { get; init; }

    /// <summary>
    /// Invoked for each complete <c>stdout</c> line (without the trailing newline).
    /// </summary>
    public Action<string>? Stdline { get; init; }

    /// <summary>
    /// Invoked for each complete <c>stderr</c> line (without the trailing newline).
    /// </summary>
    public Action<string>? Errline { get; init; }

    /// <summary>
    /// Invoked with internal debug log lines emitted by the runner (e.g. argument echo,
    /// timeout messages). Mirrors upstream <c>ExecListeners.debug</c>.
    /// </summary>
    public Action<string>? Debug { get; init; }
}
