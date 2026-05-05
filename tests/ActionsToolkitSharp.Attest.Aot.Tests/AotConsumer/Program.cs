// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using ActionsToolkitSharp.Attest;
using ActionsToolkitSharp.Attest.Auth;
using ActionsToolkitSharp.Attest.Json;
using ActionsToolkitSharp.Attest.Models;
using ActionsToolkitSharp.Attest.Services;
using ActionsToolkitSharp.Attest.Signing;
using ActionsToolkitSharp.Attest.Store;
using ActionsToolkitSharp.Octokit.Extensions;

using Microsoft.Extensions.DependencyInjection;

using Sigstore;

namespace ActionsToolkitSharp.Attest.AotConsumer;

/// <summary>
/// Native AOT dispatcher exercising the public surface of
/// <see cref="ActionsToolkitSharp.Attest"/> so the trimmer roots every API
/// path and validates the source-gen JSON pipeline.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: ats-attest-aot-consumer <case>");
        }

        try
        {
            return args[0] switch
            {
                "register-services" => RunRegisterServices(),
                "intoto-build" => RunIntotoBuild(),
                "endpoints-resolve" => RunEndpointsResolve(),
                "oidc-construct" => RunOidcConstruct(),
                "signer-factory-construct" => RunSignerFactoryConstruct(),
                "json-roundtrip" => RunJsonRoundtrip(),
                "provenance-decode" => RunProvenanceDecode(),
                _ => Fail($"Unknown case: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            return Fail($"{ex.GetType().FullName}: {ex.Message}");
        }
    }

    private static int RunRegisterServices()
    {
        using var sp = new ServiceCollection()
            .AddGitHubClientServices("token")
            .AddAttestServices()
            .BuildServiceProvider();

        var attest = sp.GetRequiredService<IAttestService>();
        var store = sp.GetRequiredService<IAttestationStore>();
        var signer = sp.GetRequiredService<IAttestSigner>();
        var pb = sp.GetRequiredService<IProvenancePredicateBuilder>();
        var oidc = sp.GetRequiredService<IOidcTokenProvider>();
        var factory = sp.GetRequiredService<SigstoreSignerFactory>();

        return Ok(
            "register-services",
            $"{attest.GetType().Name}|{store.GetType().Name}|{signer.GetType().Name}|{pb.GetType().Name}|{oidc.GetType().Name}|{factory.GetType().Name}");
    }

    private static int RunIntotoBuild()
    {
        var subject = new Subject
        {
            Name = "artifact",
            Digest = new Dictionary<string, string> { ["sha256"] = "00" },
        };
        var predicate = new Predicate
        {
            Type = "https://example.test/predicate/v1",
            Params = JsonNode.Parse("""{ "k": "v" }""")!,
        };

        var statement = InTotoStatementBuilder.Build([subject], predicate);
        var json = JsonSerializer.Serialize(statement, AttestJsonContext.Default.InTotoStatement);
        var parsed = JsonNode.Parse(json)!.AsObject();

        if (parsed["_type"]!.GetValue<string>() != "https://in-toto.io/Statement/v1")
        {
            return Fail("intoto-build _type mismatch");
        }
        if (parsed["subject"]!.AsArray().Count != 1)
        {
            return Fail("intoto-build subject count mismatch");
        }

        return Ok("intoto-build", json);
    }

    private static int RunEndpointsResolve()
    {
        var pg = EndpointsResolver.Resolve(SigstoreInstance.PublicGood);
        if (pg.FulcioUrl.Host != "fulcio.sigstore.dev")
        {
            return Fail($"endpoints-resolve public-good fulcio mismatch: {pg.FulcioUrl}");
        }

        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com");
        var gh = EndpointsResolver.Resolve(SigstoreInstance.GitHub);
        if (gh.FulcioUrl.Host != "fulcio.githubapp.com")
        {
            return Fail($"endpoints-resolve github fulcio mismatch: {gh.FulcioUrl}");
        }

        return Ok("endpoints-resolve", $"{pg.FulcioUrl} | {gh.FulcioUrl}");
    }

    private static int RunOidcConstruct()
    {
        using var http = new HttpClient();
        var provider = new GitHubActionsOidcTokenProvider(http);
        return provider is null ? Fail("oidc-construct null") : Ok("oidc-construct", provider.GetType().FullName ?? "<unknown>");
    }

    private static int RunSignerFactoryConstruct()
    {
        using var http = new HttpClient();
        var oidc = new GitHubActionsOidcTokenProvider(http);
        var factory = new SigstoreSignerFactory(oidc, () => new HttpClient());
        return Ok("signer-factory-construct", factory.GetType().FullName ?? "<unknown>");
    }

    private static int RunJsonRoundtrip()
    {
        var endpoints = new SigstoreEndpoints
        {
            FulcioUrl = new Uri("https://fulcio.sigstore.dev"),
            RekorUrl = new Uri("https://rekor.sigstore.dev"),
        };
        var json = JsonSerializer.Serialize(endpoints, AttestJsonContext.Default.SigstoreEndpoints);
        var roundtrip = JsonSerializer.Deserialize(json, AttestJsonContext.Default.SigstoreEndpoints);
        if (roundtrip is null || roundtrip.FulcioUrl.Host != "fulcio.sigstore.dev")
        {
            return Fail($"json-roundtrip mismatch: {json}");
        }

        return Ok("json-roundtrip", json);
    }

    private static int RunProvenanceDecode()
    {
        var payload = new JsonObject
        {
            ["repository"] = "owner/repo",
            ["repository_id"] = "1",
            ["repository_owner_id"] = "2",
            ["workflow_ref"] = "owner/repo/.github/workflows/main.yml@refs/heads/main",
            ["job_workflow_ref"] = "owner/repo/.github/workflows/main.yml@refs/heads/main",
            ["ref"] = "refs/heads/main",
            ["sha"] = "babca52ab0c93ae16539e5923cb0d7403b9a093b",
            ["event_name"] = "push",
            ["run_id"] = "12345",
            ["run_attempt"] = "1",
            ["runner_environment"] = "github-hosted",
        };

        static string Base64Url(byte[] bytes)
        {
            var s = Convert.ToBase64String(bytes);
            return s.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        var jwt = $"{Base64Url(Encoding.UTF8.GetBytes("""{"alg":"none"}"""))}.{Base64Url(Encoding.UTF8.GetBytes(payload.ToJsonString()))}.{Base64Url(Encoding.UTF8.GetBytes("sig"))}";

        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com");
        var builder = new GitHubActionsProvenancePredicateBuilder(
            (_, _) => Task.FromResult(jwt),
            Environment.GetEnvironmentVariable);

        var predicate = builder.BuildAsync(null).GetAwaiter().GetResult();
        if (predicate.Type != "https://slsa.dev/provenance/v1")
        {
            return Fail($"provenance-decode predicate type mismatch: {predicate.Type}");
        }

        return Ok("provenance-decode", predicate.Type);
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
