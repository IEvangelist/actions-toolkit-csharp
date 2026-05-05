// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Hand-rolled test double for <see cref="IArtifactService"/>. Each method
/// returns a scripted response and records the request for later assertion.
/// </summary>
internal sealed class FakeArtifactService : IArtifactService
{
    public Func<CreateArtifactRequest, CreateArtifactResponse>? OnCreate { get; set; }

    public Func<FinalizeArtifactRequest, FinalizeArtifactResponse>? OnFinalize { get; set; }

    public Func<ListArtifactsRequest, Internal.Twirp.ListArtifactsResponse>? OnList { get; set; }

    public Func<GetSignedArtifactUrlRequest, GetSignedArtifactUrlResponse>? OnGetSignedUrl { get; set; }

    public Func<DeleteArtifactRequest, Internal.Twirp.DeleteArtifactResponse>? OnDelete { get; set; }

    public List<CreateArtifactRequest> CreateRequests { get; } = [];
    public List<FinalizeArtifactRequest> FinalizeRequests { get; } = [];
    public List<ListArtifactsRequest> ListRequests { get; } = [];
    public List<GetSignedArtifactUrlRequest> GetSignedUrlRequests { get; } = [];
    public List<DeleteArtifactRequest> DeleteRequests { get; } = [];

    public Task<CreateArtifactResponse> CreateArtifactAsync(
        CreateArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        CreateRequests.Add(request);
        return Task.FromResult((OnCreate ?? throw new InvalidOperationException("OnCreate not set"))(request));
    }

    public Task<FinalizeArtifactResponse> FinalizeArtifactAsync(
        FinalizeArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        FinalizeRequests.Add(request);
        return Task.FromResult((OnFinalize ?? throw new InvalidOperationException("OnFinalize not set"))(request));
    }

    public Task<Internal.Twirp.ListArtifactsResponse> ListArtifactsAsync(
        ListArtifactsRequest request,
        CancellationToken cancellationToken = default)
    {
        ListRequests.Add(request);
        return Task.FromResult((OnList ?? throw new InvalidOperationException("OnList not set"))(request));
    }

    public Task<GetSignedArtifactUrlResponse> GetSignedArtifactUrlAsync(
        GetSignedArtifactUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        GetSignedUrlRequests.Add(request);
        return Task.FromResult((OnGetSignedUrl ?? throw new InvalidOperationException("OnGetSignedUrl not set"))(request));
    }

    public Task<Internal.Twirp.DeleteArtifactResponse> DeleteArtifactAsync(
        DeleteArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        DeleteRequests.Add(request);
        return Task.FromResult((OnDelete ?? throw new InvalidOperationException("OnDelete not set"))(request));
    }
}

/// <summary>
/// Fixed <see cref="IBackendIdsProvider"/> that returns a single set of
/// identifiers, useful for tests that need a deterministic backend identity.
/// </summary>
internal sealed class FakeBackendIdsProvider(string runId = "run-1", string jobId = "job-1") : IBackendIdsProvider
{
    public BackendIds Get() => new(runId, jobId);
}

/// <summary>
/// In-memory <see cref="IPublicArtifactsApi"/> used to assert the cross-workflow
/// (REST) branch of the artifact client without standing up a real HTTP listener.
/// </summary>
internal sealed class FakePublicArtifactsApi : IPublicArtifactsApi
{
    public Func<FindBy, string?, IReadOnlyList<Artifact>>? OnList { get; set; }
    public Func<FindBy, long, Uri>? OnDownload { get; set; }
    public Action<FindBy, long>? OnDelete { get; set; }

    public List<(FindBy FindBy, string? Name)> ListInvocations { get; } = [];
    public List<(FindBy FindBy, long Id)> DownloadInvocations { get; } = [];
    public List<(FindBy FindBy, long Id)> DeleteInvocations { get; } = [];

    public Task<IReadOnlyList<Artifact>> ListAsync(
        FindBy findBy,
        string? nameFilter,
        CancellationToken cancellationToken)
    {
        ListInvocations.Add((findBy, nameFilter));
        return Task.FromResult((OnList ?? throw new InvalidOperationException("OnList not set"))(findBy, nameFilter));
    }

    public Task<Uri> GetDownloadRedirectAsync(FindBy findBy, long artifactId, CancellationToken cancellationToken)
    {
        DownloadInvocations.Add((findBy, artifactId));
        return Task.FromResult((OnDownload ?? throw new InvalidOperationException("OnDownload not set"))(findBy, artifactId));
    }

    public Task DeleteAsync(FindBy findBy, long artifactId, CancellationToken cancellationToken)
    {
        DeleteInvocations.Add((findBy, artifactId));
        (OnDelete ?? throw new InvalidOperationException("OnDelete not set"))(findBy, artifactId);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Builds an <see cref="IHttpClientFactory"/> backed by a single
/// <see cref="TestHttpMessageHandler"/> so DefaultArtifactClient can route blob
/// upload/download against a scripted handler.
/// </summary>
internal sealed class TestHttpClientFactory(TestHttpMessageHandler handler) : IHttpClientFactory
{
    public TestHttpMessageHandler Handler { get; } = handler;

    public System.Net.Http.HttpClient CreateClient(string name) => new(Handler, disposeHandler: false);
}
