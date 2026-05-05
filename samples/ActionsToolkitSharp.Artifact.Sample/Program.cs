// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.IO.Compression;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .AddGitHubActionsArtifact()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();
var artifactClient = provider.GetRequiredService<IArtifactClient>();

try
{
    var artifactName = args is [var name, ..] ? name : "sample-artifact";

    using var zip = new MemoryStream();

    using (var archive = new ZipArchive(zip, ZipArchiveMode.Create, leaveOpen: true))
    {
        var entry = archive.CreateEntry("test.txt");
        await using var entryStream = new StreamWriter(entry.Open());
        await entryStream.WriteAsync("test");
    }

    zip.Position = 0;

    var response = await artifactClient.UploadArtifactAsync(artifactName, zip);

    core.WriteInfo($"Uploaded artifact {response.ArtifactId}");
}
catch (Exception ex)
{
    core.SetFailed(ex.ToString());
}
