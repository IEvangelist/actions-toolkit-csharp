// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Octokit.Extensions;

namespace ActionsToolkit.Attest.Tests;

/// <summary>
/// Mirrors the upstream <c>__tests__/index.test.ts</c> smoke checks for the
/// public surface and validates the <see cref="ServiceCollectionExtensions"/>
/// wiring.
/// </summary>
public class IndexTests
{
    [Fact(DisplayName = "registers IAttestService via AddAttestServices")]
    public void RegistersIAttestService()
    {
        using var sp = BuildProvider();

        var svc = sp.GetRequiredService<IAttestService>();
        Assert.IsType<DefaultAttestService>(svc);
    }

    [Fact(DisplayName = "registers IAttestationStore via AddAttestServices")]
    public void RegistersIAttestationStore()
    {
        using var sp = BuildProvider();

        var store = sp.GetRequiredService<IAttestationStore>();
        Assert.IsType<GitHubAttestationStore>(store);
    }

    [Fact(DisplayName = "registers IProvenancePredicateBuilder via AddAttestServices")]
    public void RegistersProvenanceBuilder()
    {
        using var sp = BuildProvider();

        var pb = sp.GetRequiredService<IProvenancePredicateBuilder>();
        Assert.IsType<GitHubActionsProvenancePredicateBuilder>(pb);
    }

    [Fact(DisplayName = "registers SigstoreSignerFactory and IAttestSigner via AddAttestServices")]
    public void RegistersSignerFactoryAndSigner()
    {
        using var sp = BuildProvider();

        var factory = sp.GetRequiredService<SigstoreSignerFactory>();
        var signer = sp.GetRequiredService<IAttestSigner>();
        Assert.NotNull(factory);
        Assert.IsType<SigstoreAttestSigner>(signer);
    }

    [Fact(DisplayName = "throws on null IServiceCollection")]
    public void ThrowsOnNullServiceCollection() =>
        Assert.Throws<ArgumentNullException>(static () =>
            ServiceCollectionExtensions.AddAttestServices(null!));

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddGitHubClientServices("token");
        services.AddAttestServices();
        return services.BuildServiceProvider();
    }
}
