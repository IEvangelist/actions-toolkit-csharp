// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// AOT-clean port of <c>@actions/artifact/src/internal/shared/config.ts</c>.
/// Surfaces the small set of process-environment lookups that the upstream
/// artifact client reads at runtime.
/// </summary>
internal static class ArtifactConfig
{
    private const string GitHubServerUrlVariable = "GITHUB_SERVER_URL";
    private const string GitHubWorkspaceVariable = "GITHUB_WORKSPACE";
    private const string DefaultGitHubServerUrl = "https://github.com";

    /// <summary>
    /// Returns true when the host pointed to by <c>GITHUB_SERVER_URL</c> is a
    /// GitHub Enterprise Server (GHES) instance — i.e. not <c>github.com</c>,
    /// not under the <c>.ghe.com</c> dogfooding domain, and not a
    /// <c>.localhost</c> sandbox. Mirrors upstream <c>isGhes()</c>.
    /// </summary>
    public static bool IsGhes()
    {
        var value = Environment.GetEnvironmentVariable(GitHubServerUrlVariable);
        if (string.IsNullOrWhiteSpace(value))
        {
            value = DefaultGitHubServerUrl;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var url))
        {
            return false;
        }

        var hostname = url.Host.TrimEnd().ToUpperInvariant();
        var isGitHubHost = string.Equals(hostname, "GITHUB.COM", StringComparison.Ordinal);
        var isGheHost = hostname.EndsWith(".GHE.COM", StringComparison.Ordinal);
        var isLocalHost = hostname.EndsWith(".LOCALHOST", StringComparison.Ordinal);

        return !isGitHubHost && !isGheHost && !isLocalHost;
    }

    /// <summary>
    /// Returns the <c>GITHUB_WORKSPACE</c> directory if set; otherwise falls
    /// back to <see cref="Environment.CurrentDirectory"/>. The upstream
    /// implementation throws when the variable is unset, but for a .NET
    /// console workflow the working directory is a sensible fallback that
    /// keeps the API usable in tests and local sandboxes.
    /// </summary>
    public static string GetGitHubWorkspaceDirectory()
    {
        var value = Environment.GetEnvironmentVariable(GitHubWorkspaceVariable);
        return string.IsNullOrWhiteSpace(value)
            ? Environment.CurrentDirectory
            : value;
    }
}
