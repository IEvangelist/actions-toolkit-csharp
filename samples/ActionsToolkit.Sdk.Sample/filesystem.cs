#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using System.Globalization;
using System.Security.Cryptography;

var root = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Directory.GetCurrentDirectory();
var destination = Path.GetFullPath("staged-source");
IO.MakeDirectory(destination);
if (args is ["stage"])
{
    var files = Glob.Create("src/ActionsToolkit.Core/*.cs").GlobFiles(root)
        .Order(StringComparer.Ordinal).ToArray();
    if (files.Length == 0)
        throw new InvalidOperationException("No files to stage.");
    var manifest = new List<string>();
    foreach (var file in files)
    {
        var target = Path.Combine(destination, Path.GetFileName(file));
        IO.Copy(file, target);
        manifest.Add($"{Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(target)))} {Path.GetFileName(target)}");
    }
    await File.WriteAllLinesAsync(Path.Combine(destination, "manifest.txt"), manifest);
    await Core.SetOutputAsync("count", files.Length.ToString(CultureInfo.InvariantCulture));
}
else
{
    var lines = await File.ReadAllLinesAsync(Path.Combine(destination, "manifest.txt"));
    if (lines.Length != int.Parse(Core.GetInput("count", required: true), CultureInfo.InvariantCulture))
        throw new InvalidOperationException("Manifest count mismatch.");
    foreach (var line in lines)
    {
        var parts = line.Split(' ', 2);
        var actual = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(destination, parts[1]))));
        if (actual != parts[0])
            throw new InvalidOperationException($"Staged hash mismatch: {parts[1]}");
    }
    await Core.Summary.AddHeading("Verified staged source").AddRaw($"{lines.Length} files match their SHA-256 manifest.", true).WriteAsync();
}
