// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkit.ToolCache.Extensions;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension methods for registering <c>ActionsToolkit.ToolCache</c>
/// services with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IToolCacheService"/> and its dependencies. Also
    /// calls <c>AddHttpClientServices()</c> from
    /// <c>ActionsToolkit.HttpClient</c> to ensure
    /// <see cref="IHttpClient"/> is available.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to extend.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddGitHubActionsToolCache(this IServiceCollection services)
    {
        services.AddHttpClientServices();

        // The default IRetryHelper uses 3 attempts and 10-20s backoff, mirroring upstream.
        services.AddSingleton<IRetryHelper>(_ => new DefaultRetryHelper());
        services.AddSingleton<IToolCacheService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpCredentialClientFactory>();
            var http = factory.CreateClient();
            var retry = sp.GetRequiredService<IRetryHelper>();
            return new DefaultToolCacheService(http, retry);
        });

        return services;
    }
}
