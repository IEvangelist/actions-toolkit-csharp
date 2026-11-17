#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using System.Text.Json.Serialization;
using System.Text.Json;
using ActionsToolkit.Attest.Models;

if (args is ["network"])
{
    using var http = Http.CreateClient();
    var result = await http.GetAsync("https://api.github.com/rate_limit", NativeJson.Default.JsonElement);
    Core.Info(result.StatusCode.ToString());
    using var cache = Cache.CreateClient();
    Core.Info(cache.IsFeatureAvailable().ToString());
    using var artifacts = Artifact.CreateClient();
    await artifacts.ListArtifactsAsync();
    using var attest = Attest.CreateClient();
    await attest.AttestProvenanceAsync(new AttestProvenanceOptions { Token = Core.GetInput("token", required: true) });
    using var tools = ToolCache.CreateClient();
    Core.Info(tools.Find("unused", "1.0.0"));
    var client = GitHubApi.CreateClient(Core.GetInput("token", required: true));
    Core.Info(client.ToString() ?? "");
}
else
{
    IO.MakeDirectory("native-surface");
    var files = Glob.Create("native-surface/*").GlobFiles().Count();
    var process = await Exec.GetOutputAsync("dotnet", ["--version"]);
    Core.Info($"AOT surface ready: {files} files, SDK {process.Stdout.Trim()}");
    await Core.SetOutputAsync("nativeSurface", "passed");
}

[JsonSerializable(typeof(JsonElement))]
internal partial class NativeJson : JsonSerializerContext;
