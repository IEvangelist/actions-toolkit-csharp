// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.IO;

namespace ActionsToolkit.Scripting;

/// <summary>Cross-platform file operations and executable lookup.</summary>
public static class IO
{
    private static readonly IOperations Service =
        ServiceBridge.CreateHelper<IOperations>(services => services.AddGitHubActionsIO());

    /// <inheritdoc cref="IOperations.Copy" />
    public static void Copy(string source, string destination, CopyOptions? options = null) =>
        Service.Copy(source, destination, options);

    /// <inheritdoc cref="IOperations.Move" />
    public static void Move(string source, string destination, MoveOptions? options = null) =>
        Service.Move(source, destination, options);

    /// <inheritdoc cref="IOperations.Remove" />
    public static void Remove(string path) => Service.Remove(path);

    /// <inheritdoc cref="IOperations.MakeDirectory" />
    public static void MakeDirectory(string path) => Service.MakeDirectory(path);

    /// <inheritdoc cref="IOperations.Which" />
    public static string Which(string tool) => Service.Which(tool);

    /// <inheritdoc cref="IOperations.FileInPath" />
    public static string[] FileInPath(string tool) => Service.FileInPath(tool);
}
