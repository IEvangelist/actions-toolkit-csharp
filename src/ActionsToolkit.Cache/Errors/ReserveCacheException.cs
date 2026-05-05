// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Errors;

/// <summary>
/// Thrown when the cache backend refuses to reserve a cache entry — typically
/// because another job is already creating the same cache, or the runner is
/// over its repo-scoped data cap. Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/src/cache.ts"><c>ReserveCacheError</c></see>.
/// </summary>
public sealed class ReserveCacheException : Exception
{
    /// <summary>Creates a new <see cref="ReserveCacheException"/>.</summary>
    public ReserveCacheException()
    {
    }

    /// <summary>Creates a new <see cref="ReserveCacheException"/>.</summary>
    /// <param name="message">A human-readable description of the reservation failure.</param>
    public ReserveCacheException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new <see cref="ReserveCacheException"/>.</summary>
    /// <param name="message">A human-readable description of the reservation failure.</param>
    /// <param name="innerException">The inner exception that triggered the failure.</param>
    public ReserveCacheException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
