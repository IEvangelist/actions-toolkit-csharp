// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.HttpClient;
using ActionsToolkit.HttpClient.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.HttpClient.AotConsumer;

/// <summary>
/// Native AOT dispatcher that exercises the public surface of
/// <see cref="ActionsToolkit.HttpClient"/>. Mirrors the upstream
/// <c>actions/toolkit/packages/http-client/__tests__/auth.test.ts</c> and
/// <c>basic.test.ts</c> concerns: factory registration plus instantiation of the
/// basic, bearer, and PAT credential clients. No network requests are made.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: actions-toolkit-httpclient-aot-consumer <case>");
        }

        try
        {
            var services = new ServiceCollection()
                .AddHttpClientServices()
                .BuildServiceProvider();
            var factory = services.GetRequiredService<IHttpCredentialClientFactory>();

            return args[0] switch
            {
                "instantiate-default-client" => RunDefaultClient(factory),
                "auth-handler-basic" => RunBasic(factory),
                "auth-handler-bearer" => RunBearer(factory),
                "auth-handler-pat" => RunPat(factory),
                _ => Fail($"Unknown case: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            return Fail($"{ex.GetType().FullName}: {ex.Message}");
        }
    }

    private static int RunDefaultClient(IHttpCredentialClientFactory factory)
    {
        using var client = factory.CreateClient();
        return client is not null
            ? Ok("instantiate-default-client", client.GetType().FullName ?? "<unknown>")
            : Fail("CreateClient returned null.");
    }

    private static int RunBasic(IHttpCredentialClientFactory factory)
    {
        using var client = factory.CreateBasicClient("aot-user", "aot-pass");
        return client is not null
            ? Ok("auth-handler-basic", client.GetType().FullName ?? "<unknown>")
            : Fail("CreateBasicClient returned null.");
    }

    private static int RunBearer(IHttpCredentialClientFactory factory)
    {
        using var client = factory.CreateBearerTokenClient("aot-bearer-token");
        return client is not null
            ? Ok("auth-handler-bearer", client.GetType().FullName ?? "<unknown>")
            : Fail("CreateBearerTokenClient returned null.");
    }

    private static int RunPat(IHttpCredentialClientFactory factory)
    {
        using var client = factory.CreatePersonalAccessTokenClient("aot-pat-token");
        return client is not null
            ? Ok("auth-handler-pat", client.GetType().FullName ?? "<unknown>")
            : Fail("CreatePersonalAccessTokenClient returned null.");
    }

    private static int Ok(string @case, string detail = "")
    {
        Console.WriteLine($"[OK] {@case}{(detail.Length > 0 ? $" {detail}" : string.Empty)}");
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine($"[FAIL] {message}");
        return 1;
    }
}
