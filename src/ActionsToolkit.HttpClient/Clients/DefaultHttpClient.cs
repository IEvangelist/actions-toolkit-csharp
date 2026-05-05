// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkit.HttpClient;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal sealed class DefaultHttpClient(NetClient client, IRequestHandler? requestHandler = null) : IHttpClient
{
    private static async ValueTask<TypedResponse<T>> ProcessResponseAsync<T>(
        HttpResponseMessage response,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        TypedResponse<T> typedResponse = new(response.StatusCode)
        {
            ResponseHttpHeaders = response.Headers
        };

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);

            typedResponse = typedResponse with
            {
                Result = result,
            };
        }

        return typedResponse;
    }

    async ValueTask<HttpResponseMessage> IHttpClient.DeleteAsync(
        string requestUri,
        Dictionary<string, IEnumerable<string>>? additionalHeaders,
        CancellationToken cancellationToken)
    {
        using var request = PrepareRequest(requestUri, HttpMethod.Delete, additionalHeaders);

        return await client.SendAsync(request, cancellationToken);
    }

    async ValueTask<TypedResponse<T>> IHttpClient.GetAsync<T>(
        string requestUri,
        JsonTypeInfo<T> jsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? additionalHeaders,
        CancellationToken cancellationToken)
    {
        using var request = PrepareRequest(requestUri, HttpMethod.Get, additionalHeaders);

        var response = await client.SendAsync(request, cancellationToken);

        return await ProcessResponseAsync(response, jsonTypeInfo, cancellationToken);
    }

    async ValueTask<HttpResponseMessage> IHttpClient.OptionsAsync(
        string requestUri,
        Dictionary<string, IEnumerable<string>>? additionalHeaders,
        CancellationToken cancellationToken)
    {
        using var request = PrepareRequest(requestUri, HttpMethod.Options, additionalHeaders);

        return await client.SendAsync(request, cancellationToken);
    }

    ValueTask<TypedResponse<TResult>> IHttpClient.PatchAsync<TData, TResult>(
        string requestUri,
        TData data,
        JsonTypeInfo<TData> dataJsonTypeInfo,
        JsonTypeInfo<TResult> resultJsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? additionalHeaders,
        CancellationToken cancellationToken)
    {
        return RequestAsync(requestUri, HttpMethod.Patch, data, dataJsonTypeInfo, resultJsonTypeInfo, additionalHeaders, cancellationToken);
    }

    ValueTask<TypedResponse<TResult>> IHttpClient.PostAsync<TData, TResult>(
        string requestUri,
        TData data,
        JsonTypeInfo<TData> dataJsonTypeInfo,
        JsonTypeInfo<TResult> resultJsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? additionalHeaders,
        CancellationToken cancellationToken)
    {
        return RequestAsync(requestUri, HttpMethod.Post, data, dataJsonTypeInfo, resultJsonTypeInfo, additionalHeaders, cancellationToken);
    }

    ValueTask<TypedResponse<TResult>> IHttpClient.PutAsync<TData, TResult>(
        string requestUri,
        TData data,
        JsonTypeInfo<TData> dataJsonTypeInfo,
        JsonTypeInfo<TResult> resultJsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? additionalHeaders,
        CancellationToken cancellationToken)
    {
        return RequestAsync(requestUri, HttpMethod.Put, data, dataJsonTypeInfo, resultJsonTypeInfo, additionalHeaders, cancellationToken);
    }

    async ValueTask<HttpResponseMessage> IHttpClient.HeadAsync(
        string requestUri,
        Dictionary<string, IEnumerable<string>>? additionalHeaders,
        CancellationToken cancellationToken)
    {
        using var request = PrepareRequest(requestUri, HttpMethod.Head, additionalHeaders);

        return await client.SendAsync(request, cancellationToken);
    }

    async ValueTask<HttpResponseMessage> IHttpClient.SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (requestHandler is not null)
        {
            var existing = request.Headers
                .ToDictionary(h => h.Key, h => h.Value);

            var prepared = requestHandler.PrepareRequestHeaders(existing);

            foreach (var (headerKey, headerValues) in prepared)
            {
                if (request.Headers.Contains(headerKey))
                {
                    request.Headers.Remove(headerKey);
                }

                request.Headers.Add(headerKey, headerValues);
            }
        }

        return await client.SendAsync(request, cancellationToken);
    }

    private async ValueTask<TypedResponse<TResult>> RequestAsync<TData, TResult>(
        string requestUri,
        HttpMethod method,
        TData data,
        JsonTypeInfo<TData> dataJsonTypeInfo,
        JsonTypeInfo<TResult> resultJsonTypeInfo,
        Dictionary<string, IEnumerable<string>>? headers,
        CancellationToken cancellationToken)
    {
        using var request = PrepareRequest(requestUri, method, headers);

        using var content = GetRequestJsonContent(data, dataJsonTypeInfo);

        request.Content = content;

        using var response = await client.SendAsync(request, cancellationToken);

        return await ProcessResponseAsync(response, resultJsonTypeInfo, cancellationToken);
    }

    private HttpRequestMessage PrepareRequest(
        string requestUri,
        HttpMethod method,
        Dictionary<string, IEnumerable<string>>? headers)
    {
        var request = new HttpRequestMessage(
            method,
            requestUri);

        var preparedHeaders = headers ?? [];

        if (requestHandler is not null)
        {
            preparedHeaders = requestHandler.PrepareRequestHeaders(preparedHeaders);
        }

        foreach (var (headerKey, headerValues) in preparedHeaders)
        {
            // Use TryAddWithoutValidation so consumers can pass headers (e.g.
            // "Accept: foo") that don't satisfy strict HttpRequestHeaders
            // parsing rules. This mirrors upstream's permissive header dict.
            request.Headers.TryAddWithoutValidation(headerKey, headerValues);
        }

        return request;
    }

    private static StringContent GetRequestJsonContent<T>(
        T data,
        JsonTypeInfo<T> jsonTypeInfo)
    {
        var json = JsonSerializer.Serialize(data, jsonTypeInfo);

        return new StringContent(
            content: json,
            encoding: Encoding.UTF8,
            mediaType: "application/json");
    }

    void IDisposable.Dispose()
    {
        client?.Dispose();
    }
}
