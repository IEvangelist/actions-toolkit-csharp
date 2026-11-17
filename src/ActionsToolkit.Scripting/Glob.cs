// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Glob;

namespace ActionsToolkit.Scripting;

/// <summary>Creates globbers using the existing typed glob implementation.</summary>
public static class Glob
{
    /// <inheritdoc cref="Globber.Create(string[])" />
    public static Globber Create(params string[] patterns) => Globber.Create(patterns);

    /// <inheritdoc cref="Globber.Create(IEnumerable{string}, IEnumerable{string}?)" />
    public static Globber Create(IEnumerable<string> patterns, IEnumerable<string>? exclusions = null) =>
        Globber.Create(patterns, exclusions);
}
