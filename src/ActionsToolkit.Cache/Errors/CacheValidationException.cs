// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Errors;

/// <summary>
/// Thrown when one of the public <see cref="ICacheClient"/> entry points is
/// called with invalid arguments — empty paths, oversized keys, or keys
/// containing forbidden characters. Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/src/cache.ts"><c>ValidationError</c></see>.
/// </summary>
public sealed class CacheValidationException : Exception
{
    /// <summary>Creates a new <see cref="CacheValidationException"/>.</summary>
    public CacheValidationException()
    {
    }

    /// <summary>Creates a new <see cref="CacheValidationException"/>.</summary>
    /// <param name="message">A human-readable description of the validation failure.</param>
    public CacheValidationException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new <see cref="CacheValidationException"/>.</summary>
    /// <param name="message">A human-readable description of the validation failure.</param>
    /// <param name="innerException">The inner exception that triggered the failure.</param>
    public CacheValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
