// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.Scripting;

internal sealed class OwnedServices<T> : IDisposable, IAsyncDisposable where T : notnull
{
    private readonly ServiceProvider _provider;
    private bool _disposed;

    internal OwnedServices(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);
        TrackingHttpClientFactory.Register(services);
        _provider = services.BuildServiceProvider();
        try
        {
            Service = _provider.GetRequiredService<T>();
        }
        catch
        {
            _provider.Dispose();
            throw;
        }
    }

    internal T Service
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return field;
        }
        private init;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _provider.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await _provider.DisposeAsync().ConfigureAwait(false);
    }
}
