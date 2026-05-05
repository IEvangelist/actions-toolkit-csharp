// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// One entry in the upload zip specification: a (source, destination) pair
/// describing exactly one file or one (empty) directory to include in the
/// archive. Mirrors upstream <c>UploadZipSpecification</c>.
/// </summary>
/// <param name="SourcePath">Absolute path on the local filesystem, or
/// <see langword="null"/> for an empty-directory entry.</param>
/// <param name="DestinationPath">Path the entry will appear at inside the zip
/// archive (always starts with the platform path separator).</param>
internal sealed record UploadZipSpecificationEntry(
    string? SourcePath,
    string DestinationPath);

/// <summary>
/// Port of <c>@actions/artifact/src/internal/upload/upload-zip-specification.ts</c>.
/// Validates the candidate root directory and file list, and produces the
/// (source, destination) entries that drive zip creation.
/// </summary>
internal static class UploadZipSpecification
{
    /// <summary>
    /// Validates the supplied <paramref name="rootDirectory"/> exists and is a
    /// directory. Mirrors <c>validateRootDirectory</c> upstream.
    /// </summary>
    public static void ValidateRootDirectory(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootDirectory);

        if (!Directory.Exists(rootDirectory))
        {
            // Mirrors upstream message verbatim.
            if (File.Exists(rootDirectory))
            {
                throw new InvalidOperationException(
                    $"The provided rootDirectory {rootDirectory} is not a valid directory");
            }

            throw new DirectoryNotFoundException(
                $"The provided rootDirectory {rootDirectory} does not exist");
        }
    }

    /// <summary>
    /// Builds the upload zip specification from a list of files relative to
    /// (or already absolute under) <paramref name="rootDirectory"/>. Mirrors
    /// <c>getUploadZipSpecification</c> upstream.
    /// </summary>
    /// <param name="filesToZip">The candidate files (or empty directories) to
    /// include in the archive.</param>
    /// <param name="rootDirectory">The directory whose contents are being
    /// uploaded; <paramref name="filesToZip"/> entries must all live under
    /// it.</param>
    public static IReadOnlyList<UploadZipSpecificationEntry> GetUploadZipSpecification(
        IEnumerable<string> filesToZip,
        string rootDirectory)
    {
        ArgumentNullException.ThrowIfNull(filesToZip);
        ArgumentException.ThrowIfNullOrEmpty(rootDirectory);

        var normalizedRoot = NormalizeDirectory(Path.GetFullPath(rootDirectory));

        var spec = new List<UploadZipSpecificationEntry>();

        foreach (var file in filesToZip)
        {
            ArgumentException.ThrowIfNullOrEmpty(file);

            if (!Path.Exists(file))
            {
                throw new FileNotFoundException(
                    $"File {file} does not exist", file);
            }

            var fullPath = Path.GetFullPath(file);
            var attrs = File.GetAttributes(fullPath);
            var isDirectory = attrs.HasFlag(FileAttributes.Directory);

            if (!fullPath.StartsWith(normalizedRoot, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"The rootDirectory: {rootDirectory} is not a parent directory of the file: {file}");
            }

            var uploadPath = fullPath[normalizedRoot.Length..];
            if (uploadPath.Length == 0 || (uploadPath[0] != Path.DirectorySeparatorChar && uploadPath[0] != Path.AltDirectorySeparatorChar))
            {
                uploadPath = Path.DirectorySeparatorChar + uploadPath;
            }

            PathAndArtifactNameValidation.ValidateFilePath(uploadPath);

            spec.Add(new UploadZipSpecificationEntry(
                isDirectory ? null : fullPath,
                uploadPath));
        }

        return spec;
    }

    private static string NormalizeDirectory(string value)
    {
        // Trim a single trailing path separator so destination paths begin
        // with `/` (or the platform separator) — matching the upstream
        // `file.replace(rootDirectory, '')` behavior.
        if (value.Length > 0 &&
            (value[^1] == Path.DirectorySeparatorChar || value[^1] == Path.AltDirectorySeparatorChar))
        {
            return value[..^1];
        }

        return value;
    }
}
