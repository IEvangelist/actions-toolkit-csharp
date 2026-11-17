// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.Scripting;

internal sealed class TrackingHttpClientFactory(IHttpClientFactory factory) : IHttpClientFactory, IDisposable
{
    private readonly List<System.Net.Http.HttpClient> _clients = [];
    private bool _disposed;

    internal static void Register(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(IHttpClientFactory));
        if (descriptor is null)
        {
            return;
        }

        // AddHttpClient registers a forwarding factory. Track clients that upstream services do not themselves dispose.
        var create = descriptor.ImplementationFactory
            ?? throw new InvalidOperationException("The HTTP module requires the standard AddHttpClient factory registration.");
        services.Remove(descriptor);
        services.AddSingleton<IHttpClientFactory>(provider => new TrackingHttpClientFactory((IHttpClientFactory)create(provider)));
    }

    public System.Net.Http.HttpClient CreateClient(string name)
    {
        lock (_clients)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var client = factory.CreateClient(name);
            _clients.Add(client);
            return client;
        }
    }

    public void Dispose()
    {
        lock (_clients)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (var client in _clients)
            {
                client.Dispose();
            }

            _clients.Clear();
        }
    }
}
