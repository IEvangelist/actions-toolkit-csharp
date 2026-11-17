// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using ActionsToolkit.HttpClient;
using ActionsToolkit.HttpClient.Extensions;

namespace ActionsToolkit.Scripting;

/// <summary>Creates independently owned HTTP clients; credentials are never stored globally.</summary>
public static class Http
{
    /// <summary>Creates an anonymous client. Dispose it after all awaited requests finish.</summary>
    public static HttpScriptClient CreateClient(string? userAgent = "actions-toolkit-csharp") =>
        Create(factory => factory.CreateClient(), userAgent);

    /// <summary>Creates a client authenticated with a username and password.</summary>
    public static HttpScriptClient CreateBasicClient(string username, string password,
        string? userAgent = "actions-toolkit-csharp") =>
        Create(factory => factory.CreateBasicClient(username, password), userAgent);

    /// <summary>Creates a client authenticated with an explicit bearer token.</summary>
    public static HttpScriptClient CreateBearerTokenClient(string token, string? userAgent = "actions-toolkit-csharp")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Create(factory => factory.CreateBearerTokenClient(token), userAgent);
    }

    /// <summary>Creates a client authenticated with an explicit personal access token.</summary>
    public static HttpScriptClient CreatePersonalAccessTokenClient(string pat, string? userAgent = "actions-toolkit-csharp")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pat);
        return Create(factory => factory.CreatePersonalAccessTokenClient(pat), userAgent);
    }

    private static HttpScriptClient Create(Func<IHttpCredentialClientFactory, IHttpClient> create, string? userAgent)
    {
        var owner = new OwnedServices<IHttpCredentialClientFactory>(services => services.AddHttpClientServices(userAgent));
        try
        {
            return new HttpScriptClient(owner, create(owner.Service));
        }
        catch
        {
            owner.Dispose();
            throw;
        }
    }
}

/// <summary>An owned toolkit HTTP client. Dispose only after its outstanding operations complete.</summary>
public sealed class HttpScriptClient : IHttpClient, IAsyncDisposable
{
    private readonly OwnedServices<IHttpCredentialClientFactory> _owner;
    private readonly IHttpClient _client;
    private bool _disposed;

    internal HttpScriptClient(OwnedServices<IHttpCredentialClientFactory> owner, IHttpClient client)
    {
        _owner = owner;
        _client = client;
    }

    /// <summary>The original HTTP interface, guarded by this owner's lifetime.</summary>
    public IHttpClient Service
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return this;
        }
    }

    private IHttpClient Client
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _client;
        }
    }

    /// <inheritdoc />
    public ValueTask<HttpResponseMessage> OptionsAsync(string requestUri,
        Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default) =>
        Client.OptionsAsync(requestUri, additionalHeaders, cancellationToken);

    /// <inheritdoc />
    public ValueTask<TypedResponse<T>> GetAsync<T>(string requestUri, JsonTypeInfo<T> jsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default) =>
        Client.GetAsync(requestUri, jsonTypeInfo, additionalHeaders, cancellationToken);

    /// <inheritdoc />
    public ValueTask<HttpResponseMessage> DeleteAsync(string requestUri,
        Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync(requestUri, additionalHeaders, cancellationToken);

    /// <inheritdoc />
    public ValueTask<TypedResponse<TResult>> PostAsync<TData, TResult>(string requestUri, TData data,
        JsonTypeInfo<TData> dataJsonTypeInfo, JsonTypeInfo<TResult> resultJsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default) =>
        Client.PostAsync(requestUri, data, dataJsonTypeInfo, resultJsonTypeInfo, additionalHeaders, cancellationToken);

    /// <inheritdoc />
    public ValueTask<TypedResponse<TResult>> PatchAsync<TData, TResult>(string requestUri, TData data,
        JsonTypeInfo<TData> dataJsonTypeInfo, JsonTypeInfo<TResult> resultJsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default) =>
        Client.PatchAsync(requestUri, data, dataJsonTypeInfo, resultJsonTypeInfo, additionalHeaders, cancellationToken);

    /// <inheritdoc />
    public ValueTask<TypedResponse<TResult>> PutAsync<TData, TResult>(string requestUri, TData data,
        JsonTypeInfo<TData> dataJsonTypeInfo, JsonTypeInfo<TResult> resultJsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default) =>
        Client.PutAsync(requestUri, data, dataJsonTypeInfo, resultJsonTypeInfo, additionalHeaders, cancellationToken);

    /// <inheritdoc />
    public ValueTask<HttpResponseMessage> HeadAsync(string requestUri,
        Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default) =>
        Client.HeadAsync(requestUri, additionalHeaders, cancellationToken);

    /// <inheritdoc />
    public ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default) =>
        Client.SendAsync(request, cancellationToken);

    /// <summary>Disposes the credential-bearing client and its module-scoped service provider.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            _client.Dispose();
        }
        finally
        {
            _owner.Dispose();
        }
    }

    /// <summary>Disposes the credential-bearing client and asynchronously disposes its service provider.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            _client.Dispose();
        }
        finally
        {
            await _owner.DisposeAsync().ConfigureAwait(false);
        }
    }
}
