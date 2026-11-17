// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Artifact;

namespace ActionsToolkit.Scripting;

/// <summary>Creates owned clients for GitHub Actions workflow artifacts.</summary>
public static class Artifact
{
    /// <summary>Creates a client using the runner's artifact environment and runtime credentials.</summary>
    public static ArtifactScriptClient CreateClient() => new();
}

/// <summary>An owned artifact client. Dispose after its asynchronous operations complete.</summary>
public sealed class ArtifactScriptClient : IArtifactClient, IDisposable, IAsyncDisposable
{
    private readonly OwnedServices<IArtifactClient> _owner = new(services => services.AddGitHubActionsArtifact());

    internal ArtifactScriptClient() { }

    /// <summary>The original artifact interface, guarded by this owner's lifetime.</summary>
    public IArtifactClient Service
    {
        get
        {
            _ = _owner.Service;
            return this;
        }
    }

    /// <inheritdoc />
    public ValueTask<UploadArtifactResponse> UploadArtifactAsync(string name, IEnumerable<string> files,
        string rootDirectory, UploadArtifactOptions? options = null, CancellationToken cancellationToken = default) =>
        _owner.Service.UploadArtifactAsync(name, files, rootDirectory, options, cancellationToken);

    /// <inheritdoc />
    public ValueTask<ListArtifactsResponse> ListArtifactsAsync(ListArtifactsOptions? options = null,
        CancellationToken cancellationToken = default) =>
        _owner.Service.ListArtifactsAsync(options, cancellationToken);

    /// <inheritdoc />
    public ValueTask<GetArtifactResponse> GetArtifactAsync(string artifactName, GetArtifactOptions? options = null,
        CancellationToken cancellationToken = default) =>
        _owner.Service.GetArtifactAsync(artifactName, options, cancellationToken);

    /// <inheritdoc />
    public ValueTask<DownloadArtifactResponse> DownloadArtifactAsync(long artifactId, DownloadArtifactOptions? options = null,
        CancellationToken cancellationToken = default) =>
        _owner.Service.DownloadArtifactAsync(artifactId, options, cancellationToken);

    /// <inheritdoc />
    public ValueTask<DeleteArtifactResponse> DeleteArtifactAsync(string artifactName, DeleteArtifactOptions? options = null,
        CancellationToken cancellationToken = default) =>
        _owner.Service.DeleteArtifactAsync(artifactName, options, cancellationToken);

    /// <summary>Disposes this module's service provider.</summary>
    public void Dispose() => _owner.Dispose();

    /// <summary>Asynchronously disposes this module's service provider.</summary>
    public ValueTask DisposeAsync() => _owner.DisposeAsync();
}
