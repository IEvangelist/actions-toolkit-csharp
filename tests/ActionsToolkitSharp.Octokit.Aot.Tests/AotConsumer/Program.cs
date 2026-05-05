// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkitSharp.Octokit;
using ActionsToolkitSharp.Octokit.Extensions;
using GitHub;
using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkitSharp.Octokit.AotConsumer;

/// <summary>
/// Native AOT dispatcher that exercises the public surface of
/// <see cref="ActionsToolkitSharp.Octokit"/>. Mirrors concerns from the upstream
/// <c>actions/toolkit/packages/github/__tests__/github.test.ts</c>: client
/// instantiation via DI and via the static <see cref="GitHubClientFactory"/>, and
/// access to the workflow <see cref="Context.Current"/>. No network requests are
/// issued — the consumer is invoked with a fake <c>GITHUB_TOKEN</c>.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: ats-octokit-aot-consumer <case>");
        }

        try
        {
            return args[0] switch
            {
                "instantiate-client" => RunInstantiateClient(),
                "instantiate-via-factory" => RunInstantiateViaFactory(),
                "context-current" => RunContextCurrent(),
                _ => Fail($"Unknown case: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            return Fail($"{ex.GetType().FullName}: {ex.Message}");
        }
    }

    private static int RunInstantiateClient()
    {
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "fake-aot-token";
        using var services = new ServiceCollection()
            .AddGitHubClientServices(token)
            .BuildServiceProvider();

        var client = services.GetRequiredService<GitHubClient>();
        return client is not null
            ? Ok("instantiate-client", client.GetType().FullName ?? "<unknown>")
            : Fail("GitHubClient resolution returned null.");
    }

    private static int RunInstantiateViaFactory()
    {
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "fake-aot-token";
        var client = GitHubClientFactory.Create(token);
        return client is not null
            ? Ok("instantiate-via-factory", client.GetType().FullName ?? "<unknown>")
            : Fail("GitHubClientFactory.Create returned null.");
    }

    private static int RunContextCurrent()
    {
        var ctx = Context.Current;
        return ctx is not null
            ? Ok("context-current", $"apiUrl={ctx.ApiUrl}")
            : Fail("Context.Current returned null.");
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
