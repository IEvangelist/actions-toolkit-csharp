// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkitSharp.Glob;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extensions for registering services with the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all the services required to interact with ActionsToolkitSharp.Glob services.
    /// Consumers should require the <see cref="IGlobPatternResolverBuilder"/>
    /// to build an <see cref="IGlobPatternResolver"/>.
    /// </summary>
    public static IServiceCollection AddGitHubActionsGlob(this IServiceCollection services)
    {
        services.AddTransient<IGlobPatternResolverBuilder, DefaultGlobPatternResolverBuilder>();

        return services;
    }
}
