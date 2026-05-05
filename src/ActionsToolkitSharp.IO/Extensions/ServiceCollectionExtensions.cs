// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkitSharp.IO;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extensions for registering services with the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all the services required to interact with the
    /// <see cref="ActionsToolkitSharp"/>.<see cref="IO"/> services. Consumers
    /// should require the <see cref="IOperations"/> to perform file system
    /// operations such as <c>cp</c>, <c>mv</c>, <c>rm -rf</c>, <c>mkdir -p</c>,
    /// and <c>which</c>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddGitHubActionsIO(this IServiceCollection services)
    {
        services.AddSingleton<IOperations, Operations>();

        return services;
    }
}
