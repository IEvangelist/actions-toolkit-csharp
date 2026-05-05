// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.IO;
using Microsoft.Extensions.DependencyInjection;

namespace ActionsToolkit.IO.AotConsumer;

/// <summary>
/// Native AOT dispatcher that exercises the public surface of
/// <see cref="ActionsToolkit.IO"/> so that the trimmer roots every API.
/// Each <c>case</c> below mirrors a concern from the upstream
/// <c>actions/toolkit/packages/io/__tests__/io.test.ts</c> suite.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Usage: actions-toolkit-io-aot-consumer <case> [sandbox-path]");
        }

        try
        {
            var services = new ServiceCollection()
                .AddGitHubActionsIO()
                .BuildServiceProvider();

            var ops = services.GetRequiredService<IOperations>();
            var sandbox = args.Length >= 2 ? args[1] : Path.Combine(Path.GetTempPath(), "actions-toolkit-io-aot-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sandbox);

            return args[0] switch
            {
                "resolve-iops" => RunResolveIops(ops),
                "mkdir-p" => RunMakeDirectory(ops, sandbox),
                "cp" => RunCopy(ops, sandbox),
                "mv" => RunMove(ops, sandbox),
                "rm" => RunRemove(ops, sandbox),
                "which" => RunWhich(ops),
                "file-in-path" => RunFileInPath(ops),
                _ => Fail($"Unknown case: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            return Fail($"{ex.GetType().FullName}: {ex.Message}");
        }
    }

    private static int RunResolveIops(IOperations ops)
    {
        if (ops is null)
        {
            return Fail("IOperations resolution returned null.");
        }

        return Ok("resolve-iops", ops.GetType().FullName ?? "<unknown>");
    }

    private static int RunMakeDirectory(IOperations ops, string sandbox)
    {
        var nested = Path.Combine(sandbox, "a", "b", "c");
        ops.MakeDirectory(nested);
        return Directory.Exists(nested)
            ? Ok("mkdir-p", nested)
            : Fail("mkdir-p directory missing.");
    }

    private static int RunCopy(IOperations ops, string sandbox)
    {
        var srcDir = Path.Combine(sandbox, "src");
        var destDir = Path.Combine(sandbox, "dst");
        Directory.CreateDirectory(srcDir);
        var srcFile = Path.Combine(srcDir, "hello.txt");
        File.WriteAllText(srcFile, "hello-aot");
        var destFile = Path.Combine(destDir, "hello.txt");
        Directory.CreateDirectory(destDir);
        ops.Copy(srcFile, destFile, new CopyOptions(Recursive: false, Force: true));
        return File.Exists(destFile) && File.ReadAllText(destFile) == "hello-aot"
            ? Ok("cp", destFile)
            : Fail("cp did not produce expected destination file.");
    }

    private static int RunMove(IOperations ops, string sandbox)
    {
        var src = Path.Combine(sandbox, "mv-src.txt");
        var dst = Path.Combine(sandbox, "mv-dst.txt");
        File.WriteAllText(src, "moveme");
        ops.Move(src, dst, new MoveOptions(Force: true));
        return File.Exists(dst) && !File.Exists(src)
            ? Ok("mv", dst)
            : Fail("mv did not move file as expected.");
    }

    private static int RunRemove(IOperations ops, string sandbox)
    {
        var target = Path.Combine(sandbox, "rm-target");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "x.txt"), "x");
        ops.Remove(target);
        return !Directory.Exists(target)
            ? Ok("rm", target)
            : Fail("rm did not remove the directory.");
    }

    private static int RunWhich(IOperations ops)
    {
        var tool = OperatingSystem.IsWindows() ? "cmd" : "sh";
        var resolved = ops.Which(tool);
        return !string.IsNullOrEmpty(resolved)
            ? Ok("which", resolved)
            : Fail($"which could not resolve '{tool}'.");
    }

    private static int RunFileInPath(IOperations ops)
    {
        var tool = OperatingSystem.IsWindows() ? "cmd" : "sh";
        var matches = ops.FileInPath(tool);
        return matches.Length > 0
            ? Ok("file-in-path", matches[0])
            : Fail($"file-in-path returned no matches for '{tool}'.");
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
