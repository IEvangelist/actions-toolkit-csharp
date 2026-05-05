// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// AOT-clean port of
/// <c>@actions/artifact/src/internal/upload/path-and-artifact-name-validation.ts</c>.
/// Rejects names and per-file paths containing characters that are unsafe to
/// round-trip across NTFS, ext4, and APFS.
/// </summary>
internal static class PathAndArtifactNameValidation
{
    /// <summary>
    /// File-path-illegal characters with their human-readable labels (used in
    /// error messages). Mirrors
    /// <c>invalidArtifactFilePathCharacters</c> upstream.
    /// </summary>
    private static readonly Dictionary<char, string> s_invalidFilePathCharacters = new()
    {
        ['"'] = " Double quote \"",
        [':'] = " Colon :",
        ['<'] = " Less than <",
        ['>'] = " Greater than >",
        ['|'] = " Vertical bar |",
        ['*'] = " Asterisk *",
        ['?'] = " Question mark ?",
        ['\r'] = " Carriage return \\r",
        ['\n'] = " Line feed \\n",
    };

    /// <summary>
    /// Artifact-name-illegal characters: the file-path set plus path
    /// separators. Mirrors <c>invalidArtifactNameCharacters</c> upstream.
    /// </summary>
    private static readonly Dictionary<char, string> s_invalidArtifactNameCharacters = BuildArtifactNameMap();

    /// <summary>
    /// Validates an artifact name. Throws
    /// <see cref="InvalidArtifactNameException"/> when null, empty, or
    /// containing any character from <see cref="s_invalidArtifactNameCharacters"/>.
    /// </summary>
    public static void ValidateArtifactName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidArtifactNameException(
                "Provided artifact name input during validation is empty",
                nameof(name));
        }

        foreach (var (key, label) in s_invalidArtifactNameCharacters)
        {
            if (name.Contains(key, StringComparison.Ordinal))
            {
                throw new InvalidArtifactNameException(
                    BuildNameError(name, label),
                    nameof(name));
            }
        }
    }

    /// <summary>
    /// Validates a per-file path (as it will appear inside the produced zip).
    /// Path separators are allowed; otherwise mirrors
    /// <see cref="ValidateArtifactName"/>.
    /// </summary>
    public static void ValidateFilePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new InvalidArtifactNameException(
                "Provided file path input during validation is empty",
                nameof(path));
        }

        foreach (var (key, label) in s_invalidFilePathCharacters)
        {
            if (path.Contains(key, StringComparison.Ordinal))
            {
                throw new InvalidArtifactNameException(
                    BuildPathError(path, label),
                    nameof(path));
            }
        }
    }

    private static Dictionary<char, string> BuildArtifactNameMap()
    {
        var map = new Dictionary<char, string>(s_invalidFilePathCharacters)
        {
            ['\\'] = " Backslash \\",
            ['/'] = " Forward slash /",
        };
        return map;
    }

    private static string BuildNameError(string name, string label)
    {
        var allLabels = string.Join(",", s_invalidArtifactNameCharacters.Values);
        return
            $"The artifact name is not valid: {name}. Contains the following character: {label}\n"
            + $"Invalid characters include: {allLabels}\n"
            + "These characters are not allowed in the artifact name due to limitations with certain file systems such as NTFS. "
            + "To maintain file system agnostic behavior, these characters are intentionally not allowed to prevent potential problems with downloads on different file systems.";
    }

    private static string BuildPathError(string path, string label)
    {
        var allLabels = string.Join(",", s_invalidFilePathCharacters.Values);
        return
            $"The path for one of the files in artifact is not valid: {path}. Contains the following character: {label}\n"
            + $"Invalid characters include: {allLabels}\n"
            + "The following characters are not allowed in files that are uploaded due to limitations with certain file systems such as NTFS. "
            + "To maintain file system agnostic behavior, these characters are intentionally not allowed to prevent potential problems with downloads on different file systems.";
    }
}
