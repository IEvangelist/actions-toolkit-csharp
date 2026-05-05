// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace ActionsToolkitSharp.IO;

/// <summary>
/// A collection of utilities for working with file system paths.
/// </summary>
public static class Utilities
{
    /// <summary>
    /// Gets a value indicating whether the current operating system is Windows.
    /// </summary>
    public static bool IsWindows { get; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Gets all of the <c>PATH</c> environment variables as a <see cref="string"/> array.
    /// </summary>
    public static Lazy<string[]> Paths { get; } = new(() =>
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        return path?.Split(Path.PathSeparator) ?? [];
    });

    /// <summary>
    /// Gets all of the <c>PATHEXT</c> environment variables as a <see cref="string"/> array.
    /// </summary>
    public static Lazy<string[]> PathExtensions { get; } = new(() =>
    {
        var pathExts = Environment.GetEnvironmentVariable("PATHEXT");
        return pathExts?.Split(Path.PathSeparator) ?? [];
    });

    ///<inheritdoc cref="File.Exists(string?)" />
    public static bool Exists(string? path)
    {
        return File.Exists(path);
    }

    ///<inheritdoc cref="Path.IsPathRooted(string?)" />
    public static bool IsRooted(string? path)
    {
        return Path.IsPathRooted(path);
    }

    /// <summary>
    /// Indicates whether the given <paramref name="path"/> resolves as a directory.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns><see langword="true"/> when <paramref name="path"/> refers to a
    /// directory, else <see langword="false"/>.</returns>
    public static bool IsDirectory(
        [NotNullWhen(true)] string? path)
    {
        return Directory.Exists(path) && (File.GetAttributes(path!) & FileAttributes.Directory) is FileAttributes.Directory;
    }

    /// <summary>
    /// Best effort attempt to determine whether a file exists and is executable.
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <param name="extensions">Additional file extensions to try</param>
    /// <returns>If file exists and is executable, returns the file path.
    /// Otherwise empty string</returns>
    public static string TryGetExecutablePath(string filePath, string[] extensions)
    {
        if (Exists(filePath))
        {
            return filePath;
        }

        var dirPath = Path.GetDirectoryName(filePath);
        if (dirPath is null)
        {
            return string.Empty;
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        foreach (var ext in extensions)
        {
            var fullPath = Path.Combine(dirPath, $"{fileName}{ext}");
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return string.Empty;
    }

#pragma warning disable IDE0051 // Remove unused private members
    private static bool IsUnixExecutable(FileInfo file)
#pragma warning restore IDE0051 // Remove unused private members
    {
        return file.Exists && (file.Attributes & FileAttributes.Archive) is FileAttributes.Archive;
    }
}
