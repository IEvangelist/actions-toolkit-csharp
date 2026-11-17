// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Exec;

namespace ActionsToolkit.Scripting;

/// <summary>Runs child processes with the toolkit's options, listeners, and failure semantics.</summary>
public static class Exec
{
    private static readonly IExecService Service =
        ServiceBridge.CreateHelper<IExecService>(services => services.AddGitHubActionsExec());

    /// <inheritdoc cref="IExecService.ExecAsync" />
    public static ValueTask<int> RunAsync(string commandLine, string[]? args = null,
        ExecOptions? options = null, CancellationToken cancellationToken = default) =>
        Service.ExecAsync(commandLine, args, options, cancellationToken);

    /// <inheritdoc cref="IExecService.GetExecOutputAsync" />
    public static ValueTask<ExecOutput> GetOutputAsync(string commandLine, string[]? args = null,
        ExecOptions? options = null, CancellationToken cancellationToken = default) =>
        Service.GetExecOutputAsync(commandLine, args, options, cancellationToken);
}
