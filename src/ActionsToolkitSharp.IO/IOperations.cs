// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.IO;

/// <summary>
/// An abstraction of the available various I/O-based operations available.
/// </summary>
public interface IOperations
{
    /// <summary>
    /// Copies a file or folder.
    /// </summary>
    /// <param name="source">The source path.</param>
    /// <param name="destination">The destination path.</param>
    /// <param name="options">(Optional) copy options.</param>
    void Copy(string source, string destination, CopyOptions? options = default);

    /// <summary>
    /// Moves a path.
    /// </summary>
    /// <param name="source">The source path.</param>
    /// <param name="destination">The destination path.</param>
    /// <param name="options">(Optional) move options.</param>
    void Move(string source, string destination, MoveOptions? options = default)
    {
        var overwrite = options?.Force ?? true;

        if (Utilities.IsDirectory(source) && Utilities.IsDirectory(destination) && overwrite)
        {
            Directory.Move(source, destination);
        }
        else
        {
            File.Move(source, destination, overwrite);
        }
    }

    /// <summary>
    /// Removes a path recursively with force
    /// </summary>
    /// <param name="path">Path to remove.</param>
    void Remove(string path)
    {
        if (Utilities.IsDirectory(path))
        {
            Directory.Delete(path, true);
        }
        else
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Make a directory. Creates the full path with folders in between.
    /// </summary>
    /// <param name="path">Path to create.</param>
    void MakeDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Returns path of a tool had the tool actually been invoked. Resolves via <c>PATH</c>.
    /// </summary>
    /// <param name="tool">Name of the tool.</param>
    /// <returns>The path to the tool.</returns>
    string Which(string tool)
    {
        if (Utilities.IsRooted(tool) && Utilities.Exists(tool))
        {
            return tool;
        }

        var matches = FileInPath(tool);
        return matches?.FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Returns a list of all occurrences of the given tool via <c>PATH</c>.
    /// </summary>
    /// <param name="tool">Name of the tool.</param>
    /// <returns>All occurrences of the given tool.</returns>
    string[] FileInPath(string tool);
}
