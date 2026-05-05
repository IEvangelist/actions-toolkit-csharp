// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Thrown when an artifact upload fails at any step of the create / blob
/// upload / finalize flow exposed by the GitHub Actions results service.
/// </summary>
public sealed class ArtifactUploadException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactUploadException"/>
    /// class with the supplied <paramref name="message"/>.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    public ArtifactUploadException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactUploadException"/>
    /// class with the supplied <paramref name="message"/> and
    /// <paramref name="innerException"/>.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    /// <param name="innerException">The underlying exception that triggered
    /// this failure.</param>
    public ArtifactUploadException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
