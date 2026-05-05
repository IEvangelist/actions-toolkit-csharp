// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Services;

/// <summary>
/// Resolves the Sigstore <see cref="SigstoreEndpoints"/> for a chosen
/// <see cref="SigstoreInstance"/>. Mirrors the <c>signingEndpoints</c>
/// function from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/endpoints.ts">
/// <c>actions/toolkit:packages/attest/src/endpoints.ts</c></a>.
/// </summary>
public static class EndpointsResolver
{
    private const string FulcioPublicGoodUrl = "https://fulcio.sigstore.dev";
    private const string RekorPublicGoodUrl = "https://rekor.sigstore.dev";
    private const string GitHubServerUrlEnvironmentVariable = "GITHUB_SERVER_URL";
    private const string DefaultGitHubServerUrl = "https://github.com";

    /// <summary>
    /// The fixed endpoints for the Sigstore public-good instance.
    /// </summary>
    public static readonly SigstoreEndpoints SigstorePublicGood = new()
    {
        FulcioUrl = new Uri(FulcioPublicGoodUrl),
        RekorUrl = new Uri(RekorPublicGoodUrl),
    };

    /// <summary>
    /// Resolves the signing endpoints for the supplied
    /// <paramref name="instance"/>. When <paramref name="instance"/> is
    /// <see langword="null"/>, the public-good instance is returned.
    /// </summary>
    /// <param name="instance">The Sigstore instance to resolve.</param>
    /// <returns>The resolved <see cref="SigstoreEndpoints"/>.</returns>
    public static SigstoreEndpoints Resolve(SigstoreInstance? instance)
    {
        return (instance ?? SigstoreInstance.PublicGood) switch
        {
            SigstoreInstance.PublicGood => SigstorePublicGood,
            SigstoreInstance.GitHub => BuildGitHubEndpoints(),
            _ => SigstorePublicGood,
        };
    }

    private static SigstoreEndpoints BuildGitHubEndpoints()
    {
        var serverUrl = Environment.GetEnvironmentVariable(GitHubServerUrlEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            serverUrl = DefaultGitHubServerUrl;
        }

        var host = new Uri(serverUrl).Host;
        if (string.Equals(host, "github.com", StringComparison.OrdinalIgnoreCase))
        {
            host = "githubapp.com";
        }

        return new SigstoreEndpoints
        {
            FulcioUrl = new Uri($"https://fulcio.{host}"),
            TsaServerUrl = new Uri($"https://timestamp.{host}"),
        };
    }
}
