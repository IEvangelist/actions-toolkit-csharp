// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Tests;

/// <summary>
/// Mirrors the upstream <c>__tests__/endpoints.test.ts</c> suite.
/// </summary>
public class EndpointsTests
{
    [Fact(DisplayName = "returns the public-good Sigstore endpoints by default")]
    public void ReturnsPublicGoodByDefault()
    {
        var endpoints = EndpointsResolver.Resolve(null);

        Assert.Equal(new Uri("https://fulcio.sigstore.dev"), endpoints.FulcioUrl);
        Assert.Equal(new Uri("https://rekor.sigstore.dev"), endpoints.RekorUrl);
        Assert.Null(endpoints.TsaServerUrl);
    }

    [Fact(DisplayName = "returns the public-good Sigstore endpoints for SigstoreInstance.PublicGood")]
    public void ReturnsPublicGoodForPublicGoodInstance()
    {
        var endpoints = EndpointsResolver.Resolve(SigstoreInstance.PublicGood);

        Assert.Equal(new Uri("https://fulcio.sigstore.dev"), endpoints.FulcioUrl);
        Assert.Equal(new Uri("https://rekor.sigstore.dev"), endpoints.RekorUrl);
    }

    [Fact(DisplayName = "returns the GitHub Sigstore endpoints derived from github.com")]
    public void ReturnsGitHubEndpointsFromGitHubCom()
    {
        var prior = Environment.GetEnvironmentVariable("GITHUB_SERVER_URL");
        try
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com");

            var endpoints = EndpointsResolver.Resolve(SigstoreInstance.GitHub);

            Assert.Equal(new Uri("https://fulcio.githubapp.com"), endpoints.FulcioUrl);
            Assert.Equal(new Uri("https://timestamp.githubapp.com"), endpoints.TsaServerUrl);
            Assert.Null(endpoints.RekorUrl);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", prior);
        }
    }

    [Fact(DisplayName = "returns the GitHub Sigstore endpoints derived from a custom GitHub host")]
    public void ReturnsGitHubEndpointsForCustomHost()
    {
        var prior = Environment.GetEnvironmentVariable("GITHUB_SERVER_URL");
        try
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://my.ghe.com");

            var endpoints = EndpointsResolver.Resolve(SigstoreInstance.GitHub);

            Assert.Equal(new Uri("https://fulcio.my.ghe.com"), endpoints.FulcioUrl);
            Assert.Equal(new Uri("https://timestamp.my.ghe.com"), endpoints.TsaServerUrl);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", prior);
        }
    }
}
