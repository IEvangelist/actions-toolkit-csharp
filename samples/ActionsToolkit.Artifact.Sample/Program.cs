// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .AddGitHubActionsArtifact()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();
var artifactClient = provider.GetRequiredService<IArtifactClient>();

try
{
    var artifactName = args is [var name, ..] ? name : "sample-artifact";

    // Stage a small set of files under a temp root, then upload them. This
    // mirrors the upstream `@actions/artifact` README's "Upload an Artifact"
    // example: build a list of file paths, then call `uploadArtifact(name,
    // files, rootDirectory)`.
    var rootDirectory = Path.Combine(Path.GetTempPath(), $"artifact-sample-{Guid.NewGuid():N}");
    Directory.CreateDirectory(rootDirectory);

    var helloPath = Path.Combine(rootDirectory, "hello.txt");
    var jsonPath = Path.Combine(rootDirectory, "data", "payload.json");
    Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);

    await File.WriteAllTextAsync(helloPath, "Hello from ActionsToolkit.Artifact!\n");
    await File.WriteAllTextAsync(jsonPath, "{\"sample\":true}");

    string[] files = [helloPath, jsonPath];

    var uploadResponse = await artifactClient.UploadArtifactAsync(
        artifactName,
        files,
        rootDirectory,
        new UploadArtifactOptions
        {
            RetentionDays = 1,
            CompressionLevel = 6,
        });

    core.WriteInfo(
        $"Uploaded artifact {uploadResponse.Id} ({uploadResponse.Size} bytes, digest={uploadResponse.Digest}).");

    // Fetch the metadata back via GetArtifact/ListArtifacts.
    var listResponse = await artifactClient.ListArtifactsAsync(new ListArtifactsOptions { Latest = true });
    core.WriteInfo($"List returned {listResponse.Artifacts.Count} artifact(s) in the current run.");

    var getResponse = await artifactClient.GetArtifactAsync(artifactName);
    core.WriteInfo($"Get returned artifact id={getResponse.Artifact.Id}, size={getResponse.Artifact.Size}.");

    // Download into a fresh directory, then optionally clean up.
    var downloadDirectory = Path.Combine(Path.GetTempPath(), $"artifact-sample-dl-{Guid.NewGuid():N}");
    var downloadResponse = await artifactClient.DownloadArtifactAsync(
        getResponse.Artifact.Id,
        new DownloadArtifactOptions { Path = downloadDirectory });
    core.WriteInfo($"Downloaded artifact into {downloadResponse.DownloadPath}.");

    var deleteResponse = await artifactClient.DeleteArtifactAsync(artifactName);
    core.WriteInfo($"Deleted artifact id={deleteResponse.Id}.");
}
catch (GhesNotSupportedException ex)
{
    core.SetFailed($"GHES is not supported: {ex.Message}");
}
catch (Exception ex)
{
    core.SetFailed(ex.ToString());
}
