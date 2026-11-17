// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Attest;
using ActionsToolkit.Attest.Auth;
using ActionsToolkit.Attest.Models;
using ActionsToolkit.Attest.Services;
using ActionsToolkit.Attest.Signing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigstore;

namespace ActionsToolkit.Scripting;

/// <summary>Creates owned attestation clients; authentication stays explicit in each operation's options.</summary>
public static class Attest
{
    /// <summary>Creates an attestation client. Actual signing requires the runner's OIDC permission and environment.</summary>
    public static AttestScriptClient CreateClient() => new();
}

/// <summary>An owned attestation service. Dispose after signing and storage operations complete.</summary>
public sealed class AttestScriptClient : IAttestService, IDisposable, IAsyncDisposable
{
    private readonly OwnedServices<IAttestService> _owner = new(Register);

    internal AttestScriptClient() { }

    /// <summary>The original attestation interface, guarded by this owner's lifetime.</summary>
    public IAttestService Service
    {
        get
        {
            _ = _owner.Service;
            return this;
        }
    }

    /// <inheritdoc />
    public ValueTask<Attestation> AttestAsync(AttestOptions options, CancellationToken cancellationToken = default) =>
        _owner.Service.AttestAsync(options, cancellationToken);

    /// <inheritdoc />
    public ValueTask<Attestation> AttestProvenanceAsync(AttestProvenanceOptions options,
        CancellationToken cancellationToken = default) =>
        _owner.Service.AttestProvenanceAsync(options, cancellationToken);

    /// <summary>Disposes this module's service provider.</summary>
    public void Dispose() => _owner.Dispose();

    /// <summary>Asynchronously disposes this module's service provider.</summary>
    public ValueTask DisposeAsync() => _owner.DisposeAsync();

    private static void Register(IServiceCollection services)
    {
        services.AddAttestServices();
        services.Replace(ServiceDescriptor.Singleton<IOidcTokenProvider>(provider =>
            new GitHubActionsOidcTokenProvider(provider.GetRequiredService<IHttpClientFactory>().CreateClient("Scripting.Oidc"))));
        services.Replace(ServiceDescriptor.Singleton<SigstoreSignerFactory>(provider =>
            new SigstoreSignerFactory(provider.GetRequiredService<IOidcTokenProvider>(),
                () => provider.GetRequiredService<IHttpClientFactory>().CreateClient("Scripting.Sigstore"))));
    }
}
