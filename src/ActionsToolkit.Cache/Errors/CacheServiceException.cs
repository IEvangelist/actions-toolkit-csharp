// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Errors;

/// <summary>
/// Thrown when the cache backend Twirp service or signed-URL transport
/// returns an unexpected status / payload that the client cannot turn into
/// a typed result.
/// </summary>
public sealed class CacheServiceException : Exception
{
    /// <summary>Creates a new <see cref="CacheServiceException"/>.</summary>
    public CacheServiceException()
    {
    }

    /// <summary>Creates a new <see cref="CacheServiceException"/>.</summary>
    /// <param name="message">A human-readable description of the protocol failure.</param>
    public CacheServiceException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new <see cref="CacheServiceException"/>.</summary>
    /// <param name="message">A human-readable description of the protocol failure.</param>
    /// <param name="innerException">The inner exception that triggered the failure.</param>
    public CacheServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
