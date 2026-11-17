// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.ToolCache;
using ActionsToolkit.ToolCache.Extensions;

namespace ActionsToolkit.Scripting;

/// <summary>Creates owned clients for downloading, extracting, and caching runner tools.</summary>
public static class ToolCache
{
    /// <summary>Creates an isolated tool-cache client using the runner's tool-cache environment.</summary>
    public static ToolCacheScriptClient CreateClient() => new();
}

/// <summary>An owned tool-cache service. Dispose after its asynchronous operations complete.</summary>
public sealed class ToolCacheScriptClient : IDisposable, IAsyncDisposable
{
    private readonly OwnedServices<IToolCacheService> _owner = new(services => services.AddGitHubActionsToolCache());

    internal ToolCacheScriptClient() { }

    /// <summary>The original service for advanced extraction and manifest operations. Do not retain beyond this owner.</summary>
    public IToolCacheService Service => _owner.Service;

    /// <inheritdoc cref="IToolCacheService.DownloadToolAsync" />
    public ValueTask<string> DownloadToolAsync(string url, string? destination = null, string? auth = null,
        IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default) =>
        Service.DownloadToolAsync(url, destination, auth, headers, cancellationToken);

    /// <inheritdoc cref="IToolCacheService.Find" />
    public string Find(string toolName, string versionSpec, string? arch = null) =>
        Service.Find(toolName, versionSpec, arch);

    /// <inheritdoc cref="IToolCacheService.CacheDirAsync" />
    public ValueTask<string> CacheDirAsync(string sourceDir, string tool, string version, string? arch = null,
        CancellationToken cancellationToken = default) =>
        Service.CacheDirAsync(sourceDir, tool, version, arch, cancellationToken);

    /// <inheritdoc cref="IToolCacheService.CacheFileAsync" />
    public ValueTask<string> CacheFileAsync(string sourceFile, string targetFile, string tool, string version,
        string? arch = null, CancellationToken cancellationToken = default) =>
        Service.CacheFileAsync(sourceFile, targetFile, tool, version, arch, cancellationToken);

    /// <inheritdoc cref="IToolCacheService.ExtractZipAsync" />
    public ValueTask<string> ExtractZipAsync(string file, string? dest = null, CancellationToken cancellationToken = default) =>
        Service.ExtractZipAsync(file, dest, cancellationToken);

    /// <inheritdoc cref="IToolCacheService.ExtractTarAsync" />
    public ValueTask<string> ExtractTarAsync(string file, string? dest = null, string[]? flags = null,
        CancellationToken cancellationToken = default) =>
        Service.ExtractTarAsync(file, dest, flags, cancellationToken);

    /// <summary>Disposes this module's service provider and HTTP clients.</summary>
    public void Dispose() => _owner.Dispose();

    /// <summary>Asynchronously disposes this module's service provider and HTTP clients.</summary>
    public ValueTask DisposeAsync() => _owner.DisposeAsync();

}
