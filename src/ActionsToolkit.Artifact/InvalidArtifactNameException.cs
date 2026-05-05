// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Thrown when an artifact name is invalid (empty, contains disallowed
/// characters, or otherwise rejected by the upstream validation rules).
/// </summary>
public sealed class InvalidArtifactNameException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidArtifactNameException"/> class.
    /// </summary>
    /// <param name="message">A description of why the name is invalid.</param>
    /// <param name="paramName">The parameter name that contained the bad
    /// value.</param>
    public InvalidArtifactNameException(string message, string? paramName = null)
        : base(message, paramName)
    {
    }
}
