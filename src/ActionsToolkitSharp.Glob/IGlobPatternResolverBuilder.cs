// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Glob;

/// <summary>
/// A builder for creating an <see cref="IGlobPatternResolver"/>.
/// Both inclusive (<see cref="WithInclusions(string[])"/>) and exclusive (<see cref="WithExclusions(string[])"/>)
/// patterns are optionally added to the builder, and then the <see cref="Build"/> method is
/// called to create a <see cref="IGlobPatternResolver"/>.
/// </summary>
public interface IGlobPatternResolverBuilder
{
    /// <summary>
    /// A fluent method for adding inclusive patterns to the builder.
    /// </summary>
    /// <param name="includePatterns">Patterns to include in the <see cref="GlobResult"/>.</param>
    /// <returns>Itself as a fluent API with the added inclusions.</returns>
    IGlobPatternResolverBuilder WithInclusions(params string[] includePatterns);

    /// <summary>
    /// A fluent method for adding exclusive patterns to the builder.
    /// </summary>
    /// <param name="excludePatterns">Patterns to exclude in the <see cref="GlobResult"/>.</param>
    /// <returns>Itself as a fluent API with the added exclusions.</returns>
    IGlobPatternResolverBuilder WithExclusions(params string[] excludePatterns);

    /// <summary>
    /// Builds the <see cref="IGlobPatternResolver"/> from the builder, with all inclusions and exclusions added.
    /// </summary>
    /// <returns>An <see cref="IGlobPatternResolver"/> instance.</returns>
    IGlobPatternResolver Build();
}
