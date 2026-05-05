// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// Thrown when an artifact operation is attempted against a GitHub Enterprise
/// Server (GHES) instance. Mirrors upstream <c>GHESNotSupportedError</c> from
/// <c>@actions/artifact</c>: the v2+ artifact APIs are not supported on GHES.
/// </summary>
public sealed class GhesNotSupportedException : Exception
{
    private const string DefaultMessage =
        "@actions/artifact v2.0.0+, upload-artifact@v4+ and download-artifact@v4+ "
        + "are not currently supported on GHES.";

    /// <summary>
    /// Initializes a new instance of the <see cref="GhesNotSupportedException"/>
    /// class with the upstream default message.
    /// </summary>
    public GhesNotSupportedException()
        : base(DefaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GhesNotSupportedException"/>
    /// class.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    public GhesNotSupportedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GhesNotSupportedException"/>
    /// class.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    /// <param name="innerException">The underlying exception that triggered
    /// this failure.</param>
    public GhesNotSupportedException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
