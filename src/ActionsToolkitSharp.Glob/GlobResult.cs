// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Glob;

/// <summary>
/// Represents the glob result, indicating whether matches were found and their corresponding <paramref name="Files"/>.
/// </summary>
/// <param name="HasMatches">The value from <see cref="GlobEvaluationResult.HasMatches"/>.</param>
/// <param name="Files">The value from <see cref="GlobEvaluationResult.Files"/>.</param>
public readonly record struct GlobResult(
    bool HasMatches,
    IEnumerable<FileInfo> Files)
{
    /// <summary>
    /// Implicitly converts the <paramref name="result"/> instance to a <see cref="GlobResult"/>.
    /// </summary>
    /// <param name="result">The result instance to convert from.</param>
    public static implicit operator GlobResult(GlobEvaluationResult result) =>
        new(
            HasMatches: result.HasMatches,
            Files: result.Files?.Select(match => match)
                ?? []);
}