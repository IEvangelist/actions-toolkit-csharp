// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Glob;

/// <summary>
/// A service that resolves glob patterns.
/// </summary>
public interface IGlobPatternResolver
{
    /// <summary>
    /// Gets the <see cref="GlobResult"/> for the the given <see cref="IGlobPatternResolverBuilder"/>
    /// that was used to create this instance.
    /// </summary>
    /// <param name="directory">The directory path to search for files.
    /// If not provided, defaults to <see cref="Directory.GetCurrentDirectory"/>.</param>
    /// <returns>A <see cref="GlobResult"/> instance representing the results of the glob operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <see langword="null"/>.</exception>
    GlobResult GetGlobResult(string? directory = null);

    /// <summary>
    /// Gets the files for the specified directory path from the given <see cref="IGlobPatternResolverBuilder"/>
    /// that was used to create this instance.
    /// </summary>
    /// <param name="directory">The directory path to search for files.
    /// If not provided, defaults to <see cref="Directory.GetCurrentDirectory"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of file paths.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <see langword="null"/>.</exception>
    IEnumerable<string> GetGlobFiles(string? directory = null);
}