// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Thrown when the GitHub Actions results service reports that the artifact
/// storage quota has been exhausted. Mirrors upstream <c>UsageError</c> from
/// <c>@actions/artifact</c>.
/// </summary>
public sealed class ArtifactUsageException : Exception
{
    private const string DefaultMessage =
        "Artifact storage quota has been hit. Unable to upload any new artifacts.\n"
        + "More info on storage limits: https://docs.github.com/en/billing/managing-billing-for-github-actions/about-billing-for-github-actions#calculating-minute-and-storage-spending";

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactUsageException"/>
    /// class with the upstream default message.
    /// </summary>
    public ArtifactUsageException()
        : base(DefaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactUsageException"/>
    /// class.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    public ArtifactUsageException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactUsageException"/>
    /// class.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    /// <param name="innerException">The underlying exception that triggered
    /// this failure.</param>
    public ArtifactUsageException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Returns true when the supplied <paramref name="message"/> is one that
    /// upstream <c>UsageError.isUsageErrorMessage</c> would classify as a
    /// storage-quota failure.
    /// </summary>
    public static bool IsUsageErrorMessage(string? message) =>
        !string.IsNullOrEmpty(message)
        && message.Contains("insufficient usage", StringComparison.Ordinal);
}
