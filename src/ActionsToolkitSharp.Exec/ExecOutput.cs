// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Exec;

/// <summary>
/// The captured output of <see cref="IExecService.GetExecOutputAsync"/>.
/// Mirrors the upstream <c>ExecOutput</c> interface from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/exec/src/interfaces.ts"><c>@actions/exec</c></a>.
/// </summary>
/// <param name="ExitCode">The exit code of the launched process.</param>
/// <param name="Stdout">The full <c>stdout</c> stream of the process as a UTF-8 string.</param>
/// <param name="Stderr">The full <c>stderr</c> stream of the process as a UTF-8 string.</param>
public readonly record struct ExecOutput(int ExitCode, string Stdout, string Stderr);
