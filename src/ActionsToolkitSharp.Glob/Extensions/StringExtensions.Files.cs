// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Glob;

/// <summary>
/// Extensions on <see cref="string"/> to support globbing.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Gets the files for the specified directory path and include patterns.
    /// </summary>
    /// <param name="directory">The directory path to search for files.
    /// If not provided, defaults to <see cref="Directory.GetCurrentDirectory"/>.</param>
    /// <param name="includePatterns">The include patterns to use when searching for files.</param>
    /// <param name="excludePatterns">The exclude patterns to use when searching for files.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of file paths.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <see langword="null"/>.</exception>
    public static IEnumerable<string> GetGlobFiles(
        this string? directory,
        IEnumerable<string> includePatterns,
        IEnumerable<string>? excludePatterns = null)
    {
        return directory.GetGlobResult(
            includePatterns, excludePatterns) is { HasMatches: true } result
            ? result.Files.Select(file => file.FullName)
            : [];
    }
}
