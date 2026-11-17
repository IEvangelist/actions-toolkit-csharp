// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;
using ActionsToolkit.HttpClient;
using ActionsToolkit.HttpClient.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.Scripting.Tests;

public sealed class ClientTests
{
    [Fact]
    public async Task ModulesCanBeUsedTogetherWithoutDuplicateResilienceOrHiddenCredentials()
    {
        using var environment = new EnvironmentScope(new()
        {
            ["ACTIONS_RUNTIME_TOKEN"] = null,
            ["ACTIONS_RESULTS_URL"] = null,
        });
        using var anonymous = Http.CreateClient();
        using var bearer = Http.CreateBearerTokenClient("independent-token");
        using var basic = Http.CreateBasicClient("user", "password");
        using var pat = Http.CreatePersonalAccessTokenClient("independent-pat");
        await using var cache = Cache.CreateClient();
        await using var artifacts = Artifact.CreateClient();
        await using var tools = ToolCache.CreateClient();
        await using var attest = Attest.CreateClient();

        Assert.False(cache.IsFeatureAvailable());
        Assert.NotNull(artifacts.Service);
        Assert.NotNull(tools.Service);
        Assert.NotNull(attest.Service);
        Assert.NotNull(anonymous.Service);
        Assert.NotNull(bearer.Service);
        Assert.NotNull(basic.Service);
        Assert.NotNull(pat.Service);
        Core.Info("All module graphs initialized independently.");
    }

    [Fact]
    public async Task OwnedClientsRejectUseAfterSynchronousAndAsynchronousDisposal()
    {
        var http = Http.CreateClient();
        var originalHttpInterface = http.Service;
        http.Dispose();
        await http.DisposeAsync();
        Assert.Throws<ObjectDisposedException>(() => _ = http.Service);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/");
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await originalHttpInterface.SendAsync(request));

        var cache = Cache.CreateClient();
        var originalCacheInterface = cache.Service;
        await cache.DisposeAsync();
        cache.Dispose();
        Assert.Throws<ObjectDisposedException>(() => originalCacheInterface.IsFeatureAvailable());

        var artifacts = Artifact.CreateClient();
        await artifacts.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await artifacts.ListArtifactsAsync());

        var tools = ToolCache.CreateClient();
        tools.Dispose();
        Assert.Throws<ObjectDisposedException>(() => tools.Find("test-tool", "1.0.0"));
        Assert.Throws<ObjectDisposedException>(() => _ = tools.Service);

        var attest = Attest.CreateClient();
        await attest.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await attest.AttestAsync(null!));
    }

    [Fact]
    public async Task PublicHttpClientsKeepCredentialsIndependentAcrossAwaitedRequests()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var uri = $"http://127.0.0.1:{endpoint.Port}/";
        var received = ReceiveRequestsAsync(listener, 3, cancellation.Token);

        using var first = Http.CreateBearerTokenClient("first-token");
        using var second = Http.CreateBearerTokenClient("second-token");
        using var anonymous = Http.CreateClient();
        foreach (var client in new[] { first, second, anonymous })
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await client.SendAsync(request, cancellation.Token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var headers = await received;
        Assert.Contains("Authorization: Bearer first-token", headers[0], StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Authorization: Bearer second-token", headers[1], StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authorization:", headers[2], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HttpAdapterPreservesCancellationAndLifetimeAcrossAwait()
    {
        var handler = new BlockingHandler();
        var owner = new OwnedServices<IHttpCredentialClientFactory>(services =>
        {
            services.AddHttpClientServices();
            services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        });
        await using var client = new HttpScriptClient(owner, owner.Service.CreateClient());
        using var cancellation = new CancellationTokenSource();
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/");

        var pending = client.SendAsync(request, cancellation.Token).AsTask();
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(pending.IsCompleted);
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ModuleOwnerDisposesFactoryCreatedClientsAndGuardsRetainedFactory(bool asynchronous)
    {
        var handler = new ResponseHandler();
        var factory = new TestHttpFactory(handler);
        var owner = new OwnedServices<IHttpClientFactory>(services =>
            services.AddSingleton<IHttpClientFactory>(_ => factory));
        var retainedFactory = owner.Service;
        var client = retainedFactory.CreateClient("test");
        using var response = await client.GetAsync(new Uri("https://example.invalid/"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(handler.Disposed);

        if (asynchronous)
        {
            await owner.DisposeAsync();
        }
        else
        {
            owner.Dispose();
        }

        owner.Dispose();
        Assert.True(handler.Disposed);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => client.GetAsync(new Uri("https://example.invalid/")));
        Assert.Throws<ObjectDisposedException>(() => retainedFactory.CreateClient("test"));
        Assert.Throws<ObjectDisposedException>(() => _ = owner.Service);
    }

    [Fact]
    public void FailedModuleInitializationDisposesAlreadyCreatedResources()
    {
        var handler = new ResponseHandler();
        Assert.Throws<InvalidOperationException>(() => new OwnedServices<object>(services =>
        {
            services.AddSingleton(_ => handler);
            services.AddSingleton<object>(provider =>
            {
                _ = provider.GetRequiredService<ResponseHandler>();
                throw new InvalidOperationException("initialization failed");
            });
        }));
        Assert.True(handler.Disposed);
    }

    [Fact]
    public void InvalidTokensAreRejectedBeforeClientCreation()
    {
        Assert.Throws<ArgumentException>(() => Http.CreateBearerTokenClient(""));
        Assert.Throws<ArgumentException>(() => Http.CreatePersonalAccessTokenClient(" "));
    }

    private static async Task<List<string>> ReceiveRequestsAsync(TcpListener listener, int count,
        CancellationToken cancellationToken)
    {
        var received = new List<string>();
        for (var index = 0; index < count; index++)
        {
            using var connection = await listener.AcceptTcpClientAsync(cancellationToken);
            await using var stream = connection.GetStream();
            using var reader = new StreamReader(stream, leaveOpen: true);
            var headers = new List<string>();
            while (await reader.ReadLineAsync(cancellationToken) is { Length: > 0 } line)
            {
                headers.Add(line);
            }

            received.Add(string.Join('\n', headers));
            await stream.WriteAsync("HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nok"u8.ToArray(),
                cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        return received;
    }

    private sealed class TestHttpFactory(ResponseHandler handler) : IHttpClientFactory
    {
        public System.Net.Http.HttpClient CreateClient(string name) => new(handler);
    }

    private sealed class ResponseHandler : HttpMessageHandler
    {
        internal bool Disposed { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class BlockingHandler : HttpMessageHandler
    {
        internal TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Started.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
