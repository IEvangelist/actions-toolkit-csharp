// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Glob;
using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.Glob.AotConsumer;

/// <summary>
/// Native AOT dispatcher that exercises the public surface of
/// <see cref="ActionsToolkit.Glob"/>. Mirrors concerns from the upstream
/// <c>actions/toolkit/packages/glob/__tests__/internal-globber.test.ts</c> suite:
/// pattern resolution via the builder, the static <see cref="Globber"/> facade,
/// and the <c>string?</c> extension methods.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: actions-toolkit-glob-aot-consumer <case> [sandbox-path]");
        }

        try
        {
            var sandbox = args.Length >= 2
                ? args[1]
                : CreateSandbox();

            return args[0] switch
            {
                "glob-builder" => RunGlobBuilder(sandbox),
                "globber-static" => RunGlobberStatic(sandbox),
                "glob-string-extension" => RunGlobStringExtension(sandbox),
                "glob-files-string-extension" => RunGlobFilesStringExtension(sandbox),
                _ => Fail($"Unknown case: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            return Fail($"{ex.GetType().FullName}: {ex.Message}");
        }
    }

    private static string CreateSandbox()
    {
        var sandbox = Path.Combine(Path.GetTempPath(), "actions-toolkit-glob-aot-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(sandbox);
        File.WriteAllText(Path.Combine(sandbox, "a.cs"), "// a");
        File.WriteAllText(Path.Combine(sandbox, "b.cs"), "// b");
        File.WriteAllText(Path.Combine(sandbox, "c.txt"), "c");
        var nested = Path.Combine(sandbox, "nested");
        Directory.CreateDirectory(nested);
        File.WriteAllText(Path.Combine(nested, "d.cs"), "// d");
        return sandbox;
    }

    private static int RunGlobBuilder(string sandbox)
    {
        using var services = new ServiceCollection()
            .AddGitHubActionsGlob()
            .BuildServiceProvider();

        var builder = services.GetRequiredService<IGlobPatternResolverBuilder>();
        var resolver = builder
            .WithInclusions("**/*.cs")
            .WithExclusions("nested/**")
            .Build();

        var result = resolver.GetGlobResult(sandbox);
        return result.HasMatches
            ? Ok("glob-builder", $"matches={result.Files.Count()}")
            : Fail("glob-builder produced no matches.");
    }

    private static int RunGlobberStatic(string sandbox)
    {
        var globber = Globber.Create("**/*.cs");
        var result = globber.Glob(sandbox);
        return result.HasMatches
            ? Ok("globber-static", $"matches={result.Files.Count()}")
            : Fail("globber-static produced no matches.");
    }

    private static int RunGlobStringExtension(string sandbox)
    {
        var result = sandbox.GetGlobResult("**/*.cs");
        return result.HasMatches
            ? Ok("glob-string-extension", $"matches={result.Files.Count()}")
            : Fail("glob-string-extension produced no matches.");
    }

    private static int RunGlobFilesStringExtension(string sandbox)
    {
        var files = sandbox.GetGlobFiles(["**/*.cs"]).ToArray();
        return files.Length > 0
            ? Ok("glob-files-string-extension", $"files={files.Length}")
            : Fail("glob-files-string-extension produced no files.");
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
