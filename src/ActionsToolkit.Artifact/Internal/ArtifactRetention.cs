// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// Port of <c>@actions/artifact/src/internal/upload/retention.ts</c>. Maps a
/// caller-supplied retention day count onto a concrete expiration timestamp,
/// clamping to the repository-configured maximum exposed via the
/// <c>GITHUB_RETENTION_DAYS</c> environment variable.
/// </summary>
internal static class ArtifactRetention
{
    private const string GitHubRetentionDaysVariable = "GITHUB_RETENTION_DAYS";

    /// <summary>
    /// Returns the absolute expiration timestamp implied by the supplied
    /// <paramref name="retentionDays"/>, or null when the caller did not
    /// request a retention period.
    /// </summary>
    public static DateTimeOffset? GetExpiration(int? retentionDays)
    {
        if (retentionDays is null or 0)
        {
            return null;
        }

        var days = retentionDays.Value;
        var maxRetentionDays = GetRepositoryMaxRetentionDays();
        if (maxRetentionDays is { } max && max < days)
        {
            days = max;
        }

        return DateTimeOffset.UtcNow.AddDays(days);
    }

    private static int? GetRepositoryMaxRetentionDays()
    {
        var value = Environment.GetEnvironmentVariable(GitHubRetentionDaysVariable);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
