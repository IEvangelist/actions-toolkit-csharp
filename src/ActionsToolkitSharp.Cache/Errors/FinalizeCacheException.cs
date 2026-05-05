// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Errors;

/// <summary>
/// Thrown when the V2 <c>FinalizeCacheEntryUpload</c> Twirp RPC returns
/// <c>ok=false</c> after a successful blob upload. Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/src/cache.ts"><c>FinalizeCacheError</c></see>.
/// </summary>
public sealed class FinalizeCacheException : Exception
{
    /// <summary>Creates a new <see cref="FinalizeCacheException"/>.</summary>
    public FinalizeCacheException()
    {
    }

    /// <summary>Creates a new <see cref="FinalizeCacheException"/>.</summary>
    /// <param name="message">A human-readable description of the finalize failure.</param>
    public FinalizeCacheException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new <see cref="FinalizeCacheException"/>.</summary>
    /// <param name="message">A human-readable description of the finalize failure.</param>
    /// <param name="innerException">The inner exception that triggered the failure.</param>
    public FinalizeCacheException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
