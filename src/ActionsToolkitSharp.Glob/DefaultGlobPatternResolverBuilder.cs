// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Glob;

/// <inheritdoc cref="IGlobPatternResolverBuilder" />
internal sealed class DefaultGlobPatternResolverBuilder : IGlobPatternResolverBuilder
{
    private readonly Lazy<List<string>> _includePatterns = new(() => []);
    private readonly Lazy<List<string>> _excludePatterns = new(() => []);

    /// <inheritdoc />
    public IGlobPatternResolver Build()
    {
        var (anyInclusions, anyExclusions) =
            (_includePatterns.IsValueCreated, _excludePatterns.IsValueCreated);

        if (!anyInclusions && !anyExclusions)
        {
            throw new ArgumentException(
                $"""
                The {nameof(IGlobPatternResolverBuilder)} must have at least one include or exclude before calling build.
                """);
        }

        return DefaultGlobPatternResolver.Factory(
            anyInclusions ? _includePatterns.Value : Enumerable.Empty<string>(),
            anyExclusions ? _excludePatterns.Value : Enumerable.Empty<string>());
    }

    /// <inheritdoc />
    public IGlobPatternResolverBuilder WithInclusions(params string[] patterns)
    {
        foreach (var include in patterns)
        {
            _includePatterns.Value.Add(include);
        }

        return this;
    }

    /// <inheritdoc />
    public IGlobPatternResolverBuilder WithExclusions(params string[] patterns)
    {
        foreach (var exclude in patterns)
        {
            _excludePatterns.Value.Add(exclude);
        }

        return this;
    }
}
