// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Exec;

/// <summary>
/// Options for invoking <see cref="IExecService.ExecAsync"/>.
/// Mirrors the upstream <c>ExecOptions</c> interface from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/exec/src/interfaces.ts"><c>@actions/exec</c></a>.
/// </summary>
public sealed class ExecOptions
{
    /// <summary>
    /// Optional working directory for the child process. Defaults to the current
    /// process's working directory.
    /// </summary>
    public string? Cwd { get; init; }

    /// <summary>
    /// Optional environment variable dictionary for the child process. When
    /// <see langword="null"/>, the child inherits the current process's environment.
    /// </summary>
    public IDictionary<string, string>? Env { get; init; }

    /// <summary>
    /// Optional bytes to write to the child process's <c>stdin</c>. The stream is
    /// closed after the bytes are written.
    /// </summary>
    public ReadOnlyMemory<byte>? Input { get; init; }

    /// <summary>
    /// When <see langword="true"/>, suppresses live mirroring of the child's
    /// <c>stdout</c> / <c>stderr</c> to <see cref="OutStream"/> / <see cref="ErrStream"/>.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool Silent { get; init; }

    /// <summary>
    /// When <see langword="true"/>, treat any output to <c>stderr</c> as a failure.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool FailOnStdErr { get; init; }

    /// <summary>
    /// When <see langword="true"/>, do not raise an exception when the child returns a
    /// non-zero exit code. Defaults to <see langword="false"/> (non-zero throws).
    /// </summary>
    public bool IgnoreReturnCode { get; init; }

    /// <summary>
    /// Optional listeners for streaming process output and debug events.
    /// </summary>
    public ExecListeners? Listeners { get; init; }

    /// <summary>
    /// Optional sink for the live console mirror of <c>stdout</c>. Defaults to
    /// <see cref="System.Console.Out"/>.
    /// </summary>
    public TextWriter? OutStream { get; init; }

    /// <summary>
    /// Optional sink for the live console mirror of <c>stderr</c>. Defaults to
    /// <see cref="System.Console.Error"/>.
    /// </summary>
    public TextWriter? ErrStream { get; init; }

    /// <summary>
    /// On Windows, when <see langword="true"/>, skips the runner's quoting/escaping of
    /// arguments. Has no effect on non-Windows platforms. Defaults to <see langword="false"/>.
    /// </summary>
    public bool WindowsVerbatimArguments { get; init; }

    /// <summary>
    /// How long, in milliseconds, to wait for the child's <c>stdio</c> streams to close
    /// after the exit event before forcibly terminating. Defaults to <c>10000</c>.
    /// </summary>
    public int Delay { get; init; } = 10_000;
}
