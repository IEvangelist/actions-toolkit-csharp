// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Errors;

/// <summary>
/// Thrown when the GitHub Actions cache backend is not reachable — most often
/// because <c>ACTIONS_RESULTS_URL</c> (V2) or <c>ACTIONS_CACHE_URL</c> (V1)
/// is unset, indicating the process is not running inside a GitHub-hosted
/// runner job.
/// </summary>
public sealed class CacheServiceUnavailableException : Exception
{
    /// <summary>Creates a new <see cref="CacheServiceUnavailableException"/>.</summary>
    public CacheServiceUnavailableException()
    {
    }

    /// <summary>Creates a new <see cref="CacheServiceUnavailableException"/>.</summary>
    /// <param name="message">A human-readable description of why the service is unavailable.</param>
    public CacheServiceUnavailableException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new <see cref="CacheServiceUnavailableException"/>.</summary>
    /// <param name="message">A human-readable description of why the service is unavailable.</param>
    /// <param name="innerException">The inner exception that triggered the failure.</param>
    public CacheServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
