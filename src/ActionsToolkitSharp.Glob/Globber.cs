// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Glob;

/// <summary>
/// A globber instance, used to get glob results or interact glob files.
/// </summary>
public class Globber
{
    private readonly IGlobPatternResolver _glob;

    private Globber(
        IEnumerable<string> inclusions,
        IEnumerable<string>? exclusions = null) =>
        _glob = new DefaultGlobPatternResolverBuilder()
            .WithInclusions((inclusions ?? []).ToArray())
            .WithExclusions((exclusions ?? []).ToArray())
            .Build();

    /// <summary>
    /// Creates a new <see cref="Globber"/> instance.
    /// </summary>
    /// <param name="inclusions">Required inclusion patterns.</param>
    /// <returns>A new <see cref="Globber"/> instance.</returns>
    public static Globber Create(params string[] inclusions)
    {
        return new(inclusions);
    }

    /// <summary>
    /// Creates a new <see cref="Globber"/> instance.
    /// </summary>
    /// <param name="inclusions">Required inclusion patterns.</param>
    /// <param name="exclusions">Optional exclusion patterns.</param>
    /// <returns>A new <see cref="Globber"/> instance.</returns>
    public static Globber Create(
        IEnumerable<string> inclusions,
        IEnumerable<string>? exclusions = null)
    {
        return new(inclusions, exclusions);
    }

    /// <inheritdoc cref="IGlobPatternResolver.GetGlobResult(string?)" />
    public GlobResult Glob(string? directory = null)
    {
        return _glob.GetGlobResult(directory);
    }

    /// <inheritdoc cref="IGlobPatternResolver.GetGlobFiles(string?)" />
    public IEnumerable<string> GlobFiles(string? directory = null)
    {
        return _glob.GetGlobFiles(directory);
    }
}
