// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Thrown when the GitHub Actions results service or the public REST API
/// returns a response that is well-formed at the transport level but does
/// not satisfy the artifact pipeline's invariants (e.g. <c>ok=false</c>,
/// missing required fields, or an unexpected status code). Mirrors upstream
/// <c>InvalidResponseError</c> from <c>@actions/artifact</c>.
/// </summary>
public sealed class InvalidArtifactResponseException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidArtifactResponseException"/> class.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    public InvalidArtifactResponseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidArtifactResponseException"/> class.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    /// <param name="innerException">The underlying exception that triggered
    /// this failure.</param>
    public InvalidArtifactResponseException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
