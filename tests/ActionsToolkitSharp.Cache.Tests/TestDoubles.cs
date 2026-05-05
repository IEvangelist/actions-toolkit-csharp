// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Fake <see cref="ICacheTwirpService"/> for orchestration tests, recording
/// every call and replaying scripted responses.
/// </summary>
internal sealed class FakeCacheTwirpService : ICacheTwirpService
{
    public List<CreateCacheEntryRequest> CreateCalls { get; } = [];
    public List<FinalizeCacheEntryUploadRequest> FinalizeCalls { get; } = [];
    public List<GetCacheEntryDownloadUrlRequest> GetCalls { get; } = [];
    public List<DeleteCacheEntryRequest> DeleteCalls { get; } = [];

    public Func<CreateCacheEntryRequest, CreateCacheEntryResponse> CreateResponder { get; set; }
        = _ => new CreateCacheEntryResponse { Ok = true, SignedUploadUrl = "https://blob/upload" };

    public Func<FinalizeCacheEntryUploadRequest, FinalizeCacheEntryUploadResponse> FinalizeResponder { get; set; }
        = _ => new FinalizeCacheEntryUploadResponse { Ok = true, EntryId = "1" };

    public Func<GetCacheEntryDownloadUrlRequest, GetCacheEntryDownloadUrlResponse> GetResponder { get; set; }
        = _ => new GetCacheEntryDownloadUrlResponse { Ok = false };

    public Task<CreateCacheEntryResponse> CreateCacheEntryAsync(
        CreateCacheEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        CreateCalls.Add(request);
        return Task.FromResult(CreateResponder(request));
    }

    public Task<FinalizeCacheEntryUploadResponse> FinalizeCacheEntryUploadAsync(
        FinalizeCacheEntryUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        FinalizeCalls.Add(request);
        return Task.FromResult(FinalizeResponder(request));
    }

    public Task<GetCacheEntryDownloadUrlResponse> GetCacheEntryDownloadUrlAsync(
        GetCacheEntryDownloadUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        GetCalls.Add(request);
        return Task.FromResult(GetResponder(request));
    }

    public Task<DeleteCacheEntryResponse> DeleteCacheEntryAsync(
        DeleteCacheEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        DeleteCalls.Add(request);
        return Task.FromResult(new DeleteCacheEntryResponse { Ok = true });
    }

    public Task<ListCacheEntriesResponse> ListCacheEntriesAsync(
        ListCacheEntriesRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new ListCacheEntriesResponse());

    public Task<LookupCacheEntryResponse> LookupCacheEntryAsync(
        LookupCacheEntryRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new LookupCacheEntryResponse { Exists = false });
}

/// <summary>
/// Fake <see cref="IHttpClientFactory"/> that always hands out the same
/// <see cref="System.Net.Http.HttpClient"/> instance, wrapping a captured
/// <see cref="TestHttpMessageHandler"/>.
/// </summary>
internal sealed class FakeHttpClientFactory : IHttpClientFactory, IDisposable
{
    private readonly System.Net.Http.HttpClient _client;

    public TestHttpMessageHandler Handler { get; }

    public FakeHttpClientFactory(TestHttpMessageHandler handler)
    {
        Handler = handler;
        _client = new System.Net.Http.HttpClient(handler);
    }

    public System.Net.Http.HttpClient CreateClient(string name) => _client;

    public void Dispose() => _client.Dispose();
}
