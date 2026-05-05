// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Glob;

public static partial class StringExtensions
{
    /// <summary>
    /// Gets the <see cref="GlobResult"/> for the specified directory path and patterns.
    /// </summary>
    /// <param name="directory">The directory path to search for files.
    /// If not provided, defaults to <see cref="Directory.GetCurrentDirectory"/>.</param>
    /// <param name="includePatterns">The include patterns to use when searching for files.</param>
    /// <param name="excludePatterns">The exclude patterns to use when searching for files.</param>
    /// <returns>A <see cref="GlobResult"/> instance representing the results of the glob operation.</returns>
    public static GlobResult GetGlobResult(
        this string? directory,
        IEnumerable<string> includePatterns,
        IEnumerable<string>? excludePatterns = null)
    {
        directory ??= Directory.GetCurrentDirectory();

        var builder = new GlobOptionsBuilder()
            .WithBasePath(directory)
            .WithPatterns([.. includePatterns]);

        if (excludePatterns is { } ignore)
        {
            builder = builder.WithIgnorePatterns([.. ignore]);
        }

        var options = builder.Build();

        return options.ExecuteEvaluation();
    }

    /// <summary>
    /// Gets the <see cref="GlobResult"/> for the specified directory path and patterns.
    /// </summary>
    /// <param name="directory">The directory path to search for files.
    /// If not provided, defaults to <see cref="Directory.GetCurrentDirectory"/>.</param>
    /// <param name="includePatterns">The include patterns to use when searching for files.</param>
    /// <returns>A <see cref="GlobResult"/> instance representing the results of the glob operation.</returns>
    public static GlobResult GetGlobResult(
        this string? directory,
        params string[] includePatterns)
    {
        return directory.GetGlobResult(includePatterns);
    }
}
