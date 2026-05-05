// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.IO.Compression;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

var runtimeToken = Environment.GetEnvironmentVariable("ACTIONS_RUNTIME_TOKEN") ?? throw new Exception("Failed to get ACTIONS_RUNTIME_TOKEN");
var resultsServiceUrl = Environment.GetEnvironmentVariable("ACTIONS_RESULTS_URL") ?? throw new Exception("Failed to get ACTIONS_RESULTS_URL");

using var artifactClient = new ArtifactClient(runtimeToken, new Uri(resultsServiceUrl));

using var stream = new MemoryStream();

using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
{
    var entry = archive.CreateEntry("test.txt");
    await using (var entryStream = new StreamWriter(entry.Open()))
    {
        await entryStream.WriteAsync("test");
    }
}

stream.Position = 0;

var response = await artifactClient.UploadArtifactAsync(args[0], stream);

Console.WriteLine($"Uploaded artifact {response.ArtifactId}");
