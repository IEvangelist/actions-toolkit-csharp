// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Thrown by list / get / download / delete operations when the requested
/// artifact cannot be located on the GitHub Actions results service. Used by
/// Phase 3b once those operations are implemented.
/// </summary>
public sealed class ArtifactNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ArtifactNotFoundException"/> class.
    /// </summary>
    /// <param name="message">A description of the missing artifact.</param>
    public ArtifactNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ArtifactNotFoundException"/> class.
    /// </summary>
    /// <param name="message">A description of the missing artifact.</param>
    /// <param name="innerException">The underlying exception that triggered
    /// this failure.</param>
    public ArtifactNotFoundException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
