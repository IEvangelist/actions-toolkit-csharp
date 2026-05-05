// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/retention.test.ts</c>: validates
/// <see cref="ArtifactRetention.GetExpiration"/>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class RetentionTests
{
    [Fact(DisplayName = "GetExpiration returns null when retentionDays is unset")]
    public void GetExpirationReturnsNullWhenUnset() =>
        Assert.Null(ArtifactRetention.GetExpiration(null));

    [Fact(DisplayName = "GetExpiration returns null when retentionDays is zero")]
    public void GetExpirationReturnsNullWhenZero() =>
        Assert.Null(ArtifactRetention.GetExpiration(0));

    [Fact(DisplayName = "GetExpiration honors caller-supplied retention when no max is set")]
    public void GetExpirationHonorsCallerWithoutMax()
    {
        using var _ = new EnvironmentScope("GITHUB_RETENTION_DAYS", value: null);

        var expiration = ArtifactRetention.GetExpiration(7);

        Assert.NotNull(expiration);
        Assert.InRange(
            expiration!.Value - DateTimeOffset.UtcNow,
            TimeSpan.FromDays(6.99),
            TimeSpan.FromDays(7.01));
    }

    [Fact(DisplayName = "GetExpiration clamps to repository maximum when caller exceeds it")]
    public void GetExpirationClampsToRepositoryMax()
    {
        using var _ = new EnvironmentScope("GITHUB_RETENTION_DAYS", "30");

        var expiration = ArtifactRetention.GetExpiration(60);

        Assert.NotNull(expiration);
        Assert.InRange(
            expiration!.Value - DateTimeOffset.UtcNow,
            TimeSpan.FromDays(29.99),
            TimeSpan.FromDays(30.01));
    }

    [Fact(DisplayName = "GetExpiration honors caller when below repository maximum")]
    public void GetExpirationHonorsCallerWhenBelowMax()
    {
        using var _ = new EnvironmentScope("GITHUB_RETENTION_DAYS", "60");

        var expiration = ArtifactRetention.GetExpiration(7);

        Assert.NotNull(expiration);
        Assert.InRange(
            expiration!.Value - DateTimeOffset.UtcNow,
            TimeSpan.FromDays(6.99),
            TimeSpan.FromDays(7.01));
    }
}
