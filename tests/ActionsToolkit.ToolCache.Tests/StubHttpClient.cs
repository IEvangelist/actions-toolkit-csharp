// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace ActionsToolkit.ToolCache.Tests;

/// <summary>
/// Minimal hand-rolled <see cref="IHttpClient"/> backed by a
/// <see cref="System.Net.Http.HttpClient"/> with a scripted
/// <see cref="HttpMessageHandler"/>. Avoids dragging the full
/// <see cref="ActionsToolkit.HttpClient.Clients"/> stack into unit tests.
/// </summary>
internal sealed class StubHttpClient(System.Net.Http.HttpClient inner) : IHttpClient
{
    public List<HttpRequestMessage> Sent { get; } = [];

    public ValueTask<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        Sent.Add(request);
        return new ValueTask<HttpResponseMessage>(inner.SendAsync(request, cancellationToken));
    }

    public async ValueTask<TypedResponse<T>> GetAsync<T>(
        string requestUri,
        JsonTypeInfo<T> jsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? additionalHeaders = null,
        CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
        if (additionalHeaders is not null)
        {
            foreach (var (k, v) in additionalHeaders)
            {
                req.Headers.TryAddWithoutValidation(k, v);
            }
        }
        Sent.Add(req);
        var response = await inner.SendAsync(req, cancellationToken).ConfigureAwait(false);
        var typed = new TypedResponse<T>(response.StatusCode)
        {
            ResponseHttpHeaders = response.Headers,
        };
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken).ConfigureAwait(false);
            typed = typed with { Result = result };
        }
        return typed;
    }

    public ValueTask<HttpResponseMessage> OptionsAsync(string requestUri, Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    public ValueTask<HttpResponseMessage> DeleteAsync(string requestUri, Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    public ValueTask<TypedResponse<TResult>> PostAsync<TData, TResult>(string requestUri, TData data, JsonTypeInfo<TData> dataJsonTypeInfo, JsonTypeInfo<TResult> resultJsonTypeInfo, Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    public ValueTask<TypedResponse<TResult>> PatchAsync<TData, TResult>(string requestUri, TData data, JsonTypeInfo<TData> dataJsonTypeInfo, JsonTypeInfo<TResult> resultJsonTypeInfo, Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    public ValueTask<TypedResponse<TResult>> PutAsync<TData, TResult>(string requestUri, TData data, JsonTypeInfo<TData> dataJsonTypeInfo, JsonTypeInfo<TResult> resultJsonTypeInfo, Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    public ValueTask<HttpResponseMessage> HeadAsync(string requestUri, Dictionary<string, IEnumerable<string>>? additionalHeaders = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public void Dispose() => inner.Dispose();
}
