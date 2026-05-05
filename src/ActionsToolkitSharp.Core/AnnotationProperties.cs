// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core;

/// <summary>
/// Inspired by <a href="https://github.com/actions/toolkit/blob/main/packages/core/src/core.ts#L40"></a>
/// </summary>
public readonly record struct AnnotationProperties
{
    /// <summary>
    /// A title for the annotation.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// The path of the file for which the annotation should be created.
    /// </summary>
    public string? File { get; init; }

    /// <summary>
    /// The start line of the annotation.
    /// </summary>
    public int? StartLine { get; init; }

    /// <summary>
    /// The end line of the annotation.
    /// </summary>
    public int? EndLine { get; init; }

    /// <summary>
    /// The start column of the annotation.
    /// </summary>
    public int? StartColumn { get; init; }

    /// <summary>
    /// The end column of the annotation.
    /// </summary>
    public int? EndColumn { get; init; }

    private bool IsEmpty => Equals(default);

    /// <summary>
    /// Converts the current annotations instance into a readonly dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, string> ToCommandProperties()
    {
        if (IsEmpty)
        {
            return new Dictionary<string, string>();
        }

        var properties = new Dictionary<string, string>();
        TryAddProperty(properties, "title", Title);
        TryAddProperty(properties, "file", File);
        TryAddProperty(properties, "line", StartLine);
        TryAddProperty(properties, "endLine", EndLine);
        TryAddProperty(properties, "col", StartColumn);
        TryAddProperty(properties, "endColumn", EndColumn);

        return properties;

        static void TryAddProperty(
            in IDictionary<string, string> properties, string key, object? value)
        {
            if (value is not null)
            {
                properties[key] = value.ToString()!;
            }
        }
    }
}

