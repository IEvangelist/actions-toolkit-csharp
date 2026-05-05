// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Thrown when a network-level error occurs while communicating with the
/// GitHub Actions results service or the signed blob URL. Mirrors upstream
/// <c>NetworkError</c> from <c>@actions/artifact</c>.
/// </summary>
public sealed class ArtifactNetworkException : Exception
{
    private static readonly HashSet<string> s_networkCodes = new(StringComparer.Ordinal)
    {
        "ECONNRESET",
        "ENOTFOUND",
        "ETIMEDOUT",
        "ECONNREFUSED",
        "EHOSTUNREACH",
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactNetworkException"/>
    /// class.
    /// </summary>
    /// <param name="code">A short identifier for the underlying failure
    /// (typically the BSD-style errno code mirrored from upstream).</param>
    public ArtifactNetworkException(string code)
        : base(BuildMessage(code))
    {
        ArgumentNullException.ThrowIfNull(code);
        Code = code;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactNetworkException"/>
    /// class.
    /// </summary>
    /// <param name="code">A short identifier for the underlying failure.</param>
    /// <param name="innerException">The underlying exception that triggered
    /// this failure.</param>
    public ArtifactNetworkException(string code, Exception? innerException)
        : base(BuildMessage(code), innerException)
    {
        ArgumentNullException.ThrowIfNull(code);
        Code = code;
    }

    /// <summary>
    /// The mnemonic network failure code (e.g. <c>ECONNRESET</c>).
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Returns true if the supplied <paramref name="code"/> is one of the
    /// well-known transient network failure codes recognized by upstream
    /// <c>NetworkError.isNetworkErrorCode</c>.
    /// </summary>
    public static bool IsNetworkErrorCode(string? code) =>
        !string.IsNullOrEmpty(code) && s_networkCodes.Contains(code);

    private static string BuildMessage(string code) =>
        $"Unable to make request: {code}\nIf you are using self-hosted runners, "
        + "please make sure your runner has access to all GitHub endpoints: "
        + "https://docs.github.com/en/actions/hosting-your-own-runners/managing-self-hosted-runners/about-self-hosted-runners#communication-between-self-hosted-runners-and-github";
}
