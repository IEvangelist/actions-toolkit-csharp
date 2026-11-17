// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.Scripting;

internal static class ServiceBridge
{
    // Only non-disposable, self-contained helper graphs may outlive this provider.
    internal static T CreateHelper<T>(Action<IServiceCollection> register) where T : notnull
    {
        var services = new ServiceCollection();
        register(services);
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<T>();
    }
}
