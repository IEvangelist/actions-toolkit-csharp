#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using System.Security.Cryptography;

const string version = "1.8.1";
const string expected = "020468de7539ce70ef1bceaf7cde2e8c4f2ca6c3afb84642aabc5c97d9fc2a0d";
using var tools = ToolCache.CreateClient();
if (args is ["install"])
{
    var downloaded = await tools.DownloadToolAsync("https://github.com/jqlang/jq/releases/download/jq-1.8.1/jq-linux-amd64");
    var digest = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(downloaded))).ToLowerInvariant();
    if (digest != expected)
        throw new InvalidOperationException("Pinned jq SHA-256 verification failed.");
    if (!OperatingSystem.IsLinux())
        throw new PlatformNotSupportedException("This pinned tool demonstration requires Linux x64.");
    File.SetUnixFileMode(downloaded, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    var cached = await tools.CacheFileAsync(downloaded, "jq", "csharp-showcase-jq", version);
    await Core.SetOutputAsync("version", version);
    await Core.SetOutputAsync("path", cached);
}
else
{
    if (Core.GetInput("version", required: true) != version)
        throw new InvalidOperationException("Unexpected tool version input.");
    var found = tools.Find("csharp-showcase-jq", version);
    if (found != Core.GetInput("path", required: true))
        throw new InvalidOperationException("Tool cache did not resolve the producer's path.");
    var output = await Exec.GetOutputAsync(Path.Combine(found, "jq"), ["--version"]);
    if (output.Stdout.Trim() != $"jq-{version}")
        throw new InvalidOperationException("Unexpected tool output.");
    await Core.Summary.AddHeading("Verified tool cache")
        .AddRaw($"Downloaded pinned jq {version}, verified SHA-256 `{expected}`, and reused it in a later C# step.", true).WriteAsync();
}
