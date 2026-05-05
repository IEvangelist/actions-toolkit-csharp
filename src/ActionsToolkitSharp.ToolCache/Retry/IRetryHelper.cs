// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.ToolCache.Retry;

/// <summary>
/// Retry helper abstraction. Mirrors upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/src/retry-helper.ts">
/// <c>RetryHelper</c></a> from <c>@actions/tool-cache</c>.
/// </summary>
public interface IRetryHelper
{
    /// <summary>
    /// Executes <paramref name="action"/> up to <c>maxAttempts</c> times,
    /// waiting a randomized delay between <c>minSeconds</c> and
    /// <c>maxSeconds</c> between attempts. <paramref name="isRetryable"/>
    /// can short-circuit retries for non-recoverable errors. Mirrors
    /// upstream <c>RetryHelper.execute</c>.
    /// </summary>
    /// <typeparam name="T">The action result type.</typeparam>
    /// <param name="action">The async action to (re)execute.</param>
    /// <param name="isRetryable">Optional predicate; returning
    /// <see langword="false"/> aborts retries with the original error.</param>
    /// <param name="cancellationToken">Cancellation propagated to delays.</param>
    ValueTask<T> ExecuteAsync<T>(
        Func<CancellationToken, ValueTask<T>> action,
        Func<Exception, bool>? isRetryable = null,
        CancellationToken cancellationToken = default);
}
