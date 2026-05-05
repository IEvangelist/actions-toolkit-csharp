// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Sigstore;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkit.Attest;
#pragma warning restore IDE0130

/// <summary>
/// Extensions on <see cref="IServiceCollection"/> for registering the
/// <c>ActionsToolkit.Attest</c> services. Mirrors the public surface of
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/index.ts">
/// <c>actions/toolkit:packages/attest/src/index.ts</c></a>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all the services required to generate GitHub artifact attestations
    /// via <see cref="IAttestService"/>:
    /// <list type="bullet">
    ///   <item><see cref="IAttestService"/> (default
    ///   <see cref="DefaultAttestService"/>).</item>
    ///   <item><see cref="IOidcTokenProvider"/> (default
    ///   <see cref="GitHubActionsOidcTokenProvider"/>).</item>
    ///   <item><see cref="SigstoreSignerFactory"/>.</item>
    ///   <item><see cref="IAttestSigner"/> (default
    ///   <see cref="SigstoreAttestSigner"/>).</item>
    ///   <item><see cref="IAttestationStore"/> (default
    ///   <see cref="GitHubAttestationStore"/>).</item>
    ///   <item><see cref="IProvenancePredicateBuilder"/> (default
    ///   <see cref="GitHubActionsProvenancePredicateBuilder"/>).</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Callers must also register the Octokit context (via
    /// <c>AddGitHubClientServices</c>) and an
    /// <see cref="IHttpClientFactory"/> (via <c>AddHttpClient</c>).
    /// </remarks>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddAttestServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient(GitHubAttestationStore.HttpClientName, ConfigureAttestationHttpClient);

        services.TryAddSingleton<Context>(static _ => Context.Current);
        services.TryAddSingleton<IGitHubClientFactory, DefaultGitHubClientFactory>();

        services.TryAddSingleton<IOidcTokenProvider, GitHubActionsOidcTokenProvider>();
        services.TryAddSingleton<SigstoreSignerFactory>(static sp =>
            new SigstoreSignerFactory(
                sp.GetRequiredService<IOidcTokenProvider>(),
                () => new NetClient()));

        services.TryAddSingleton<IAttestSigner, SigstoreAttestSigner>();
        services.TryAddSingleton<IAttestationStore, GitHubAttestationStore>();
        services.TryAddSingleton<IProvenancePredicateBuilder, GitHubActionsProvenancePredicateBuilder>();
        services.TryAddSingleton<IAttestService, DefaultAttestService>();

        return services;
    }

    private static void ConfigureAttestationHttpClient(NetClient client)
    {
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(GitHubAttestationStore.UserAgent);
    }
}
