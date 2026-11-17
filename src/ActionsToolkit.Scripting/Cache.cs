// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Cache;
using ActionsToolkit.Cache.Models;
using ActionsToolkit.Cache.Services;

namespace ActionsToolkit.Scripting;

/// <summary>Creates owned clients for the GitHub Actions cache service.</summary>
public static class Cache
{
    /// <summary>Creates a client using the runner's cache environment and runtime credentials.</summary>
    public static CacheScriptClient CreateClient() => new();
}

/// <summary>An owned cache client. Dispose after its asynchronous operations complete.</summary>
public sealed class CacheScriptClient : ICacheClient, IDisposable, IAsyncDisposable
{
    private readonly OwnedServices<ICacheClient> _owner = new(services => services.AddCacheServices());

    internal CacheScriptClient() { }

    /// <summary>The original cache interface, guarded by this owner's lifetime.</summary>
    public ICacheClient Service
    {
        get
        {
            _ = _owner.Service;
            return this;
        }
    }

    /// <inheritdoc />
    public ValueTask<CacheEntry?> SaveCacheAsync(IReadOnlyList<string> paths, string key,
        SaveCacheOptions? options = null, bool enableCrossOsArchive = false, CancellationToken cancellationToken = default) =>
        _owner.Service.SaveCacheAsync(paths, key, options, enableCrossOsArchive, cancellationToken);

    /// <inheritdoc />
    public ValueTask<string?> RestoreCacheAsync(IReadOnlyList<string> paths, string primaryKey,
        IReadOnlyList<string>? restoreKeys = null, RestoreCacheOptions? options = null,
        bool enableCrossOsArchive = false, CancellationToken cancellationToken = default) =>
        _owner.Service.RestoreCacheAsync(paths, primaryKey, restoreKeys, options, enableCrossOsArchive, cancellationToken);

    /// <inheritdoc />
    public bool IsFeatureAvailable() => _owner.Service.IsFeatureAvailable();

    /// <summary>Disposes this module's service provider.</summary>
    public void Dispose() => _owner.Dispose();

    /// <summary>Asynchronously disposes this module's service provider.</summary>
    public ValueTask DisposeAsync() => _owner.DisposeAsync();
}
