// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

using ActionsToolkit.Cache;
using ActionsToolkit.Cache.Errors;
using ActionsToolkit.Cache.Internal;
using ActionsToolkit.Cache.Internal.Twirp;
using ActionsToolkit.Cache.Json;
using ActionsToolkit.Cache.Models;
using ActionsToolkit.Cache.Services;

using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.Cache.AotConsumer;

/// <summary>
/// Native AOT dispatcher that exercises the public surface of
/// <c>ActionsToolkit.Cache</c>. Mirrors the save / restore / lookup
/// entry points so the trimmer/AOT analyzer can prove the entire client is
/// reachable without dynamic codegen warnings.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: actions-toolkit-cache-aot-consumer <case>");
        }

        try
        {
            return args[0] switch
            {
                "compose-client" => ComposeClient(),
                "options-records" => OptionsRecords(),
                "is-feature-available" => IsFeatureAvailable(),
                "key-validation" => KeyValidation(),
                "version-computation" => VersionComputation(),
                "tar-roundtrip" => TarRoundTrip(),
                "json-roundtrip" => JsonRoundTrip(),
                _ => Fail($"Unknown case: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            return Fail($"{ex.GetType().FullName}: {ex.Message}");
        }
    }

    private static int ComposeClient()
    {
        using var provider = new ServiceCollection()
            .AddCacheServices()
            .BuildServiceProvider();

        _ = provider.GetRequiredService<ICacheClient>();
        return Ok("compose-client");
    }

    private static int OptionsRecords()
    {
        var save = new SaveCacheOptions
        {
            UploadConcurrency = 4,
            UploadChunkSize = 32 * 1024 * 1024,
            ArchiveSizeBytes = 12345,
        };
        var restore = new RestoreCacheOptions
        {
            LookupOnly = true,
            DownloadConcurrency = 8,
            TimeoutInMs = 30_000,
            SegmentTimeoutInMs = 600_000,
        };
        var entry = new CacheEntry("k", "v", 100, DateTimeOffset.UtcNow);

        if (save.UploadConcurrency != 4) return Fail("save concurrency");
        if (!restore.LookupOnly) return Fail("restore lookupOnly");
        if (entry.Key != "k") return Fail("entry key");
        if (entry.Size != 100) return Fail("entry size");
        return Ok("options-records");
    }

    private static int IsFeatureAvailable()
    {
        Environment.SetEnvironmentVariable("ACTIONS_CACHE_SERVICE_V2", "1");
        Environment.SetEnvironmentVariable("ACTIONS_RESULTS_URL", "https://results.example.com/");
        Environment.SetEnvironmentVariable("ACTIONS_RUNTIME_TOKEN", "ghs_aot");
        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com");
        try
        {
            using var provider = new ServiceCollection()
                .AddCacheServices()
                .BuildServiceProvider();

            var client = provider.GetRequiredService<ICacheClient>();
            if (!client.IsFeatureAvailable())
            {
                return Fail("is-feature-available: expected true");
            }

            return Ok("is-feature-available");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ACTIONS_CACHE_SERVICE_V2", null);
            Environment.SetEnvironmentVariable("ACTIONS_RESULTS_URL", null);
            Environment.SetEnvironmentVariable("ACTIONS_RUNTIME_TOKEN", null);
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
        }
    }

    private static int KeyValidation()
    {
        Environment.SetEnvironmentVariable("ACTIONS_CACHE_SERVICE_V2", "1");
        Environment.SetEnvironmentVariable("ACTIONS_RESULTS_URL", "https://results.example.com/");
        Environment.SetEnvironmentVariable("ACTIONS_RUNTIME_TOKEN", "ghs_aot");
        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com");
        try
        {
            using var provider = new ServiceCollection()
                .AddCacheServices()
                .BuildServiceProvider();

            var client = provider.GetRequiredService<ICacheClient>();
            try
            {
                client.SaveCacheAsync(["bin"], "bad,key").AsTask().GetAwaiter().GetResult();
                return Fail("key-validation: expected CacheValidationException");
            }
            catch (CacheValidationException)
            {
                return Ok("key-validation");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("ACTIONS_CACHE_SERVICE_V2", null);
            Environment.SetEnvironmentVariable("ACTIONS_RESULTS_URL", null);
            Environment.SetEnvironmentVariable("ACTIONS_RUNTIME_TOKEN", null);
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
        }
    }

    private static int VersionComputation()
    {
        var v1 = CacheUtils.GetCacheVersion(["bin"], CompressionMethod.ZstdWithoutLong);
        var v2 = CacheUtils.GetCacheVersion(["bin"], CompressionMethod.ZstdWithoutLong);
        var v3 = CacheUtils.GetCacheVersion(["bin"], CompressionMethod.Gzip);

        if (v1.Length != 64) return Fail("version-computation: expected 64-char hex");
        if (!string.Equals(v1, v2, StringComparison.Ordinal))
        {
            return Fail("version-computation: not deterministic");
        }
        if (string.Equals(v1, v3, StringComparison.Ordinal))
        {
            return Fail("version-computation: zstd vs gzip should differ");
        }
        return Ok("version-computation");
    }

    private static int TarRoundTrip()
    {
        var work = Path.Combine(Path.GetTempPath(), "actions-toolkit-cache-aot-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(work);
        try
        {
            var src = Path.Combine(work, "src");
            Directory.CreateDirectory(src);
            File.WriteAllText(Path.Combine(src, "f.txt"), "aot-cache-payload");

            var archive = Path.Combine(work, "cache.tzst");
            CacheTar.CreateAsync(archive, src, ["f.txt"]).AsTask().GetAwaiter().GetResult();

            var dest = Path.Combine(work, "dest");
            Directory.CreateDirectory(dest);
            CacheTar.ExtractAsync(archive, dest).AsTask().GetAwaiter().GetResult();

            var roundtripped = File.ReadAllText(Path.Combine(dest, "f.txt"));
            if (!string.Equals(roundtripped, "aot-cache-payload", StringComparison.Ordinal))
            {
                return Fail("tar-roundtrip: payload mismatch");
            }
            return Ok("tar-roundtrip");
        }
        finally
        {
            try { Directory.Delete(work, recursive: true); } catch { /* best effort */ }
        }
    }

    private static int JsonRoundTrip()
    {
        var request = new CreateCacheEntryRequest
        {
            Key = "k",
            Version = "v",
        };
        var json = JsonSerializer.Serialize(request, CacheJsonContext.Default.CreateCacheEntryRequest);

        var roundtripped = JsonSerializer.Deserialize(
            json, CacheJsonContext.Default.CreateCacheEntryRequest);
        if (roundtripped is null || roundtripped.Key != "k" || roundtripped.Version != "v")
        {
            return Fail($"json-roundtrip: bad shape '{json}'");
        }

        var responseJson = """{ "ok": true, "signed_upload_url": "https://blob/u" }""";
        var response = JsonSerializer.Deserialize(
            responseJson, CacheJsonContext.Default.CreateCacheEntryResponse);
        if (response is null || !response.Ok || response.SignedUploadUrl != "https://blob/u")
        {
            return Fail("json-roundtrip: bad response");
        }
        return Ok("json-roundtrip");
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
