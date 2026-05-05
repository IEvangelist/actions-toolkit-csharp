// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.IO;

/// <inheritdoc cref="IOperations" />
internal sealed class Operations : IOperations
{
    /// <inheritdoc cref="IOperations.Copy(string, string, CopyOptions?)" />
    public void Copy(string sourcePath, string destinationPath, CopyOptions? options = default)
    {
        CopyAll(sourcePath, destinationPath, options);
    }

    private static void CopyAll(string sourcePath, string destinationPath, CopyOptions? options = default)
    {
        var (recursive, force, copySourceDirectory) = (options ??= new(false));

        if (File.Exists(sourcePath))
        {
            if (File.Exists(destinationPath) is false || force)
            {
                File.Copy(sourcePath, destinationPath, true);
            }
        }
        else if (Directory.Exists(sourcePath))
        {
            if (recursive is false)
            {
                throw new InvalidOperationException(
                    $"Failed to copy. " +
                    $"{sourcePath} is a directory, but tried to copy without recursive flag.");
            }

            var source = new DirectoryInfo(sourcePath);
            var destination = new DirectoryInfo(destinationPath);
            destination = copySourceDirectory
                ? new(Path.Combine(destinationPath, Path.GetFileName(sourcePath)!))
                : destination;

            if (destination.Exists is false)
            {
                destination.Create();
            }

            foreach (var dir in source.GetDirectories())
            {
                var subdir = destination.CreateSubdirectory(dir.Name);
                CopyAll(dir.FullName, subdir.FullName, options);
            }

            foreach (var file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(destination.FullName, file.Name), force);
            }
        }
    }

    /// <inheritdoc cref="IOperations.FileInPath(string)" />
    public string[] FileInPath(string tool)
    {
        ArgumentException.ThrowIfNullOrEmpty(tool);

        var extensions = Utilities.PathExtensions.Value;
        if (Utilities.IsRooted(tool))
        {
            var filePath = Utilities.TryGetExecutablePath(tool, extensions);
            return filePath is { Length: > 0 } ? [filePath] : [];
        }

        if (tool.Contains(Path.PathSeparator))
        {
            return [];
        }

        var paths = Utilities.Paths.Value;
        var matches = new List<string>();
        for (var i = 0; i < paths.Length; ++i)
        {
            var path = paths[i];
            var filePath = Utilities.TryGetExecutablePath(Path.Combine(path, tool), extensions);
            if (filePath is { Length: > 0 })
            {
                matches.Add(filePath);
            }
        }

        return [.. matches];
    }
}
