// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Tests;

/// <summary>
/// Mirrors the upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/__tests__/retry-helper.test.ts">
/// <c>retry-helper.test.ts</c></a> suite.
/// </summary>
public sealed class RetryHelperTests
{
    [Fact(DisplayName = "first attempt succeeds")]
    public async Task FirstAttemptSucceeds()
    {
        var attempts = 0;
        var helper = new DefaultRetryHelper(
            maxAttempts: 3, minSeconds: 0, maxSeconds: 0,
            sleep: _ => Task.CompletedTask, info: _ => { });

        var result = await helper.ExecuteAsync<int>(_ =>
        {
            attempts++;
            return new ValueTask<int>(42);
        });

        Assert.Equal(42, result);
        Assert.Equal(1, attempts);
    }

    [Fact(DisplayName = "second attempt succeeds")]
    public async Task SecondAttemptSucceeds()
    {
        var attempts = 0;
        var helper = new DefaultRetryHelper(
            maxAttempts: 3, minSeconds: 0, maxSeconds: 0,
            sleep: _ => Task.CompletedTask, info: _ => { });

        var result = await helper.ExecuteAsync<int>(_ =>
        {
            attempts++;
            if (attempts < 2) throw new InvalidOperationException("transient");
            return new ValueTask<int>(7);
        });

        Assert.Equal(7, result);
        Assert.Equal(2, attempts);
    }

    [Fact(DisplayName = "third attempt succeeds")]
    public async Task ThirdAttemptSucceeds()
    {
        var attempts = 0;
        var helper = new DefaultRetryHelper(
            maxAttempts: 3, minSeconds: 0, maxSeconds: 0,
            sleep: _ => Task.CompletedTask, info: _ => { });

        var result = await helper.ExecuteAsync<int>(_ =>
        {
            attempts++;
            if (attempts < 3) throw new InvalidOperationException("transient");
            return new ValueTask<int>(13);
        });

        Assert.Equal(13, result);
        Assert.Equal(3, attempts);
    }

    [Fact(DisplayName = "all attempts fail")]
    public async Task AllAttemptsFail()
    {
        var attempts = 0;
        var helper = new DefaultRetryHelper(
            maxAttempts: 3, minSeconds: 0, maxSeconds: 0,
            sleep: _ => Task.CompletedTask, info: _ => { });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await helper.ExecuteAsync<int>(_ =>
            {
                attempts++;
                throw new InvalidOperationException("boom");
            });
        });
        Assert.Equal(3, attempts);
    }

    [Fact(DisplayName = "checks retryable")]
    public async Task ChecksRetryable()
    {
        var attempts = 0;
        var helper = new DefaultRetryHelper(
            maxAttempts: 3, minSeconds: 0, maxSeconds: 0,
            sleep: _ => Task.CompletedTask, info: _ => { });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await helper.ExecuteAsync<int>(
                _ =>
                {
                    attempts++;
                    throw new InvalidOperationException("non-retryable");
                },
                isRetryable: _ => false);
        });
        Assert.Equal(1, attempts);
    }

    [Fact(DisplayName = "min seconds equals max seconds")]
    public void MinSecondsEqualsMaxSeconds()
    {
        var helper = new DefaultRetryHelper(
            maxAttempts: 3, minSeconds: 5, maxSeconds: 5,
            sleep: _ => Task.CompletedTask, info: _ => { });

        Assert.NotNull(helper);
    }
}
