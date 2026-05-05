// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkitSharp.Artifact;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extensions on <see cref="IServiceCollection"/> for registering the
/// <c>ActionsToolkitSharp.Artifact</c> services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The environment variable that carries the per-job runtime token
    /// issued by GitHub Actions. The token is a JWT whose <c>scp</c> claim
    /// encodes the backend identifiers used by every Twirp RPC.
    /// </summary>
    private const string RuntimeTokenEnvironmentVariable = "ACTIONS_RUNTIME_TOKEN";

    /// <summary>
    /// The environment variable that carries the base URL of the GitHub
    /// Actions results service.
    /// </summary>
    private const string ResultsUrlEnvironmentVariable = "ACTIONS_RESULTS_URL";

    /// <summary>
    /// Adds all the services required to interact with the
    /// <see cref="ActionsToolkitSharp"/>.<see cref="Artifact"/> services.
    /// Consumers should require <see cref="IArtifactClient"/> to upload
    /// artifacts. The Twirp transport is read from
    /// <c>ACTIONS_RESULTS_URL</c> / <c>ACTIONS_RUNTIME_TOKEN</c> at the time
    /// the underlying named <see cref="NetClient"/> is materialized by
    /// <see cref="IHttpClientFactory"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddGitHubActionsArtifact(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClientServices();

        services.TryAddSingleton<IBackendIdsProvider, EnvironmentBackendIdsProvider>();

        services.AddHttpClient(DefaultArtifactService.HttpClientName, ConfigureArtifactHttpClient)
                .AddStandardResilienceHandler();

        services.TryAddSingleton<IArtifactService>(static sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient(DefaultArtifactService.HttpClientName);
            return new DefaultArtifactService(client);
        });

        services.TryAddSingleton<IArtifactClient, DefaultArtifactClient>();

        return services;
    }

    private static void ConfigureArtifactHttpClient(NetClient client)
    {
        var resultsUrl = Environment.GetEnvironmentVariable(ResultsUrlEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(resultsUrl) &&
            Uri.TryCreate(resultsUrl, UriKind.Absolute, out var baseAddress))
        {
            client.BaseAddress = baseAddress;
        }

        var runtimeToken = Environment.GetEnvironmentVariable(RuntimeTokenEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(runtimeToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", runtimeToken);
        }

        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("ActionsToolkitSharp.Artifact", productVersion: null));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
