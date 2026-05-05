// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Extensions;

/// <summary>
/// Extensions for registering services with the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all the services required to interact with GitHubClientFactory Action workflows.
    /// Consumers should require the <see cref="ICoreService"/> to interact with the workflow.
    /// </summary>
    public static IServiceCollection AddGitHubActionsCore(this IServiceCollection services)
    {
        services.AddSingleton<IConsole, DefaultConsole>();
        services.AddTransient<ICommandIssuer, DefaultCommandIssuer>();
        services.AddTransient<IFileCommandIssuer>(
            _ => new DefaultFileCommandIssuer(
                async (filePath, message) =>
                {
                    using var writer = new StreamWriter(filePath, append: true, Encoding.UTF8);
                    await writer.WriteLineAsync(message);
                }));
        services.AddTransient<ICoreService, DefaultCoreService>();

        return services;
    }
}
