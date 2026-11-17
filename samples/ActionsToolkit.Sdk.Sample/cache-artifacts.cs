#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using System.Globalization;
using System.Security.Cryptography;
using ActionsToolkit.Artifact;

var key = $"csharp-showcase-{Environment.GetEnvironmentVariable("GITHUB_RUN_ID")}-{Environment.GetEnvironmentVariable("GITHUB_RUN_ATTEMPT")}";
using var cache = Cache.CreateClient();
using var artifacts = Artifact.CreateClient();
var data = Path.GetFullPath(".showcase/roundtrip");
var downloaded = Path.GetFullPath(".showcase/downloaded");
if (args is ["produce"])
{
    IO.MakeDirectory(data);
    await File.WriteAllTextAsync(Path.Combine(data, "result.txt"), "C# cache and artifact round trip");
    var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(data, "result.txt"))));
    await cache.SaveCacheAsync([data], key);
    var uploaded = await artifacts.UploadArtifactAsync(key, [Path.Combine(data, "result.txt")], data);
    await Core.SetOutputAsync("artifactId", uploaded.Id.ToString(CultureInfo.InvariantCulture));
    await Core.SetOutputAsync("sha256", hash);
    await Core.SetOutputAsync("key", key);
}
else
{
    key = Core.GetInput("key", required: true);
    IO.Remove(data);
    var hit = await cache.RestoreCacheAsync([data], key);
    if (hit != key)
        throw new InvalidOperationException("The saved cache did not round-trip.");
    var expected = Core.GetInput("sha256", required: true);
    var cachedHash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(data, "result.txt"))));
    var id = long.Parse(Core.GetInput("artifactId", required: true), CultureInfo.InvariantCulture);
    await artifacts.DownloadArtifactAsync(id, new DownloadArtifactOptions { Path = downloaded });
    var artifactHash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(downloaded, "result.txt"))));
    if (cachedHash != expected || artifactHash != expected)
        throw new InvalidOperationException("Cache/artifact content digest mismatch.");
    await Core.Summary.AddMarkdownHeading("Cache and artifact round trip").AddNewLine()
        .AddRawMarkdown($"Cache hit: `{hit}`. Artifact `{id}` and cache both match SHA-256 `{expected}`.", true).WriteAsync();
}
