// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkit.Exec;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extensions for registering <c>ActionsToolkit.Exec</c> services with the
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IExecService"/> (and the default implementation) so that
    /// consumers can inject a process-execution service. <see cref="IExecService"/> is
    /// transient because each <see cref="System.Diagnostics.Process"/> invocation is
    /// independent and short-lived.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddGitHubActionsExec(this IServiceCollection services)
    {
        services.AddTransient<IExecService, DefaultExecService>();

        return services;
    }
}
