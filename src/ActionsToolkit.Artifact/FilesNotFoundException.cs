// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Thrown by upload validation when no files are matched. Mirrors upstream
/// <c>FilesNotFoundError</c> from <c>@actions/artifact</c>.
/// </summary>
public sealed class FilesNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesNotFoundException"/>
    /// class with no candidate files.
    /// </summary>
    public FilesNotFoundException()
        : this([])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesNotFoundException"/>
    /// class with the supplied candidate file list.
    /// </summary>
    /// <param name="files">The candidate files that could not be located.</param>
    public FilesNotFoundException(IReadOnlyList<string> files)
        : base(BuildMessage(files))
    {
        ArgumentNullException.ThrowIfNull(files);
        Files = files;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesNotFoundException"/>
    /// class with the supplied <paramref name="message"/>.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    public FilesNotFoundException(string message)
        : base(message)
    {
        Files = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesNotFoundException"/>
    /// class.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    /// <param name="innerException">The underlying exception that triggered
    /// this failure.</param>
    public FilesNotFoundException(string message, Exception? innerException)
        : base(message, innerException)
    {
        Files = [];
    }

    /// <summary>
    /// The candidate file paths that the upload pipeline could not locate.
    /// </summary>
    public IReadOnlyList<string> Files { get; }

    private static string BuildMessage(IReadOnlyList<string> files)
    {
        const string baseMessage = "No files were found to upload";
        return files.Count > 0
            ? $"{baseMessage}: {string.Join(", ", files)}"
            : baseMessage;
    }
}
