// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Thrown when the <c>ACTIONS_RUNTIME_TOKEN</c> environment variable is
/// missing, malformed, or does not contain the expected <c>Actions.Results</c>
/// scope claim from which the workflow run / job backend identifiers are
/// derived.
/// </summary>
public sealed class InvalidArtifactTokenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidArtifactTokenException"/> class.
    /// </summary>
    /// <param name="message">A description of why the token is invalid.</param>
    public InvalidArtifactTokenException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidArtifactTokenException"/> class.
    /// </summary>
    /// <param name="message">A description of why the token is invalid.</param>
    /// <param name="innerException">The underlying exception that triggered
    /// this failure.</param>
    public InvalidArtifactTokenException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
