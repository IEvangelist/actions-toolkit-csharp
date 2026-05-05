// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.ToolCache.Retry;

/// <summary>
/// Default <see cref="IRetryHelper"/> implementation. Mirrors upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/src/retry-helper.ts">
/// <c>RetryHelper</c></a> from <c>@actions/tool-cache</c>: 3 attempts by
/// default, with a uniformly-random 10-20s delay between attempts and a
/// retryability predicate that defaults to "always retry".
/// </summary>
internal sealed class DefaultRetryHelper(
    int maxAttempts = 3,
    int minSeconds = 10,
    int maxSeconds = 20,
    Func<int, Task>? sleep = null,
    Action<string>? info = null) : IRetryHelper
{
    private readonly int _maxAttempts = maxAttempts >= 1
        ? maxAttempts
        : throw new ArgumentOutOfRangeException(
            nameof(maxAttempts), "max attempts should be greater than or equal to 1");

    private readonly int _minSeconds = minSeconds >= 0
        ? minSeconds
        : throw new ArgumentOutOfRangeException(nameof(minSeconds));

    private readonly int _maxSeconds = maxSeconds >= minSeconds
        ? maxSeconds
        : throw new ArgumentOutOfRangeException(
            nameof(maxSeconds), "min seconds should be less than or equal to max seconds");

    private readonly Func<int, Task> _sleep = sleep
        ?? (s => Task.Delay(TimeSpan.FromSeconds(s)));

    private readonly Action<string> _info = info ?? Console.Out.WriteLine;

    public async ValueTask<T> ExecuteAsync<T>(
        Func<CancellationToken, ValueTask<T>> action,
        Func<Exception, bool>? isRetryable = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        var attempt = 1;
        while (attempt < _maxAttempts)
        {
            try
            {
                return await action(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception err) when (err is not OperationCanceledException)
            {
                if (isRetryable is not null && !isRetryable(err))
                {
                    throw;
                }

                _info(err.Message);
            }

            var seconds = GetSleepAmount();
            _info($"Waiting {seconds} seconds before trying again");
            await _sleep(seconds).ConfigureAwait(false);
            attempt++;
        }

        return await action(cancellationToken).ConfigureAwait(false);
    }

    private int GetSleepAmount() =>
        _minSeconds == _maxSeconds
            ? _minSeconds
            : Random.Shared.Next(_minSeconds, _maxSeconds + 1);
}
