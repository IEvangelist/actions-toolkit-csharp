// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkitSharp.Artifact;
using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkitSharp.Artifact.AotConsumer;

/// <summary>
/// Native AOT dispatcher that exercises the public surface of
/// <c>ActionsToolkitSharp.Artifact</c>. Mirrors the upload, list, get,
/// download, and delete entry points so the trimmer/AOT analyzer can prove
/// the entire client is reachable without dynamic codegen warnings.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: ats-artifact-aot-consumer <case>");
        }

        try
        {
            return args[0] switch
            {
                "compose-client" => ComposeClient(),
                "options-records" => OptionsRecords(),
                "ghes-guard" => GhesGuard(),
                "validation-helpers" => ValidationHelpers(),
                "zip-pipeline" => ZipPipeline(),
                "config-helpers" => ConfigHelpers(),
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
            .AddGitHubActionsArtifact()
            .BuildServiceProvider();

        _ = provider.GetRequiredService<IArtifactClient>();
        return Ok("compose-client");
    }

    private static int OptionsRecords()
    {
        var upload = new UploadArtifactOptions
        {
            RetentionDays = 7,
            CompressionLevel = 6,
        };
        var list = new ListArtifactsOptions { Latest = true };
        var get = new GetArtifactOptions();
        var download = new DownloadArtifactOptions { Path = "/tmp", SkipDecompress = false };
        var delete = new DeleteArtifactOptions();
        var findBy = new FindBy("token", 1, "owner", "repo");

        if (upload.RetentionDays != 7) return Fail("upload retention");
        if (!list.Latest) return Fail("list latest");
        if (get.FindBy is not null) return Fail("get findby default");
        if (download.Path is null) return Fail("download path");
        if (delete.FindBy is not null) return Fail("delete findby default");
        if (findBy.RepositoryName != "repo") return Fail("findby repo");

        return Ok("options-records");
    }

    private static int GhesGuard()
    {
        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://my-ghes.example.com");
        try
        {
            using var provider = new ServiceCollection()
                .AddGitHubActionsArtifact()
                .BuildServiceProvider();

            var client = provider.GetRequiredService<IArtifactClient>();

            try
            {
                client.ListArtifactsAsync().AsTask().GetAwaiter().GetResult();
                return Fail("ghes-guard: expected GhesNotSupportedException");
            }
            catch (GhesNotSupportedException)
            {
                return Ok("ghes-guard");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
        }
    }

    private static int ValidationHelpers()
    {
        try
        {
            // Public exception path through DefaultArtifactClient.UploadArtifactAsync
            // — invalid name should throw InvalidArtifactNameException.
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com");
            using var provider = new ServiceCollection()
                .AddGitHubActionsArtifact()
                .BuildServiceProvider();

            var client = provider.GetRequiredService<IArtifactClient>();
            try
            {
                client.UploadArtifactAsync("bad/name", [], Path.GetTempPath())
                    .AsTask().GetAwaiter().GetResult();
                return Fail("validation-helpers: expected InvalidArtifactNameException");
            }
            catch (InvalidArtifactNameException)
            {
                return Ok("validation-helpers");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
        }
    }

    private static int ZipPipeline()
    {
        // Touch the upload pipeline using empty file list to confirm
        // FilesNotFoundException flows cleanly through AOT'd code.
        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com");
        try
        {
            using var provider = new ServiceCollection()
                .AddGitHubActionsArtifact()
                .BuildServiceProvider();

            var client = provider.GetRequiredService<IArtifactClient>();
            try
            {
                client.UploadArtifactAsync("name", [], Path.GetTempPath())
                    .AsTask().GetAwaiter().GetResult();
                return Fail("zip-pipeline: expected FilesNotFoundException");
            }
            catch (FilesNotFoundException)
            {
                return Ok("zip-pipeline");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
        }
    }

    private static int ConfigHelpers()
    {
        // Touch the public DownloadArtifactResponse / GetArtifactResponse /
        // ListArtifactsResponse / DeleteArtifactResponse types to ensure
        // their serializable shapes are reachable.
        var artifact = new Artifact("a", 1, 100, DateTimeOffset.UtcNow, "sha256:abc");
        var list = new ListArtifactsResponse([artifact]);
        var get = new GetArtifactResponse(artifact);
        var download = new DownloadArtifactResponse("/tmp", DigestMismatch: false);
        var delete = new DeleteArtifactResponse(1);
        var upload = new UploadArtifactResponse(1, 100, "sha256:abc");

        if (list.Artifacts.Count != 1) return Fail("config-helpers list");
        if (get.Artifact.Id != 1) return Fail("config-helpers get");
        if (download.DownloadPath != "/tmp") return Fail("config-helpers download");
        if (delete.Id != 1) return Fail("config-helpers delete");
        if (upload.Size != 100) return Fail("config-helpers upload");

        return Ok("config-helpers");
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
