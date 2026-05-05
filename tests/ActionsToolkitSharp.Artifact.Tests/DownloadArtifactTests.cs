// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/download-artifact.test.ts</c>. Builds a tiny
/// in-memory zip, scripts the blob handler to return it, and asserts the
/// extracted contents land in the requested directory.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class DownloadArtifactTests : IDisposable
{
    private readonly EnvironmentScope _serverScope =
        new("GITHUB_SERVER_URL", "https://github.com");
    private readonly string _root;

    public DownloadArtifactTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "ats-download-artifact-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        _serverScope.Dispose();
        if (Directory.Exists(_root))
        {
            try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
        }
    }

    [Fact(DisplayName = "Throws GhesNotSupportedException on a GHES host")]
    public async Task ThrowsOnGhes()
    {
        using var ghesScope = new EnvironmentScope("GITHUB_SERVER_URL", "https://my-ghes.example.com");

        var client = BuildClient(out _, out _, out _);

        await Assert.ThrowsAsync<GhesNotSupportedException>(
            () => client.DownloadArtifactAsync(1).AsTask());
    }

    [Fact(DisplayName = "Throws ArtifactNotFoundException when no artifact matches")]
    public async Task ThrowsWhenNotFound()
    {
        var client = BuildClient(out var service, out _, out _);
        service.OnList = _ => new Internal.Twirp.ListArtifactsResponse
        {
            Artifacts = [],
        };

        await Assert.ThrowsAsync<ArtifactNotFoundException>(
            () => client.DownloadArtifactAsync(1).AsTask());
    }

    [Fact(DisplayName = "Downloads, extracts, and returns the download path")]
    public async Task DownloadsAndExtracts()
    {
        var zipBytes = BuildZip(("hello.txt", "hello"));

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(zipBytes),
        });
        var client = BuildClient(out var service, out _, out _, handler);
        service.OnList = _ => new Internal.Twirp.ListArtifactsResponse
        {
            Artifacts =
            [
                new MonolithArtifact
                {
                    WorkflowRunBackendId = "run-1",
                    WorkflowJobRunBackendId = "job-1",
                    DatabaseId = 1,
                    Name = "name",
                    Size = zipBytes.Length,
                },
            ],
        };
        service.OnGetSignedUrl = _ => new GetSignedArtifactUrlResponse
        {
            SignedUrl = "https://blob.example.com/artifact.zip",
        };

        var response = await client.DownloadArtifactAsync(
            1, new DownloadArtifactOptions { Path = _root });

        Assert.Equal(_root, response.DownloadPath);
        Assert.True(File.Exists(Path.Combine(_root, "hello.txt")));
        Assert.Equal("hello", await File.ReadAllTextAsync(Path.Combine(_root, "hello.txt")));
    }

    [Fact(DisplayName = "SkipDecompress writes the raw zip without extracting it")]
    public async Task SkipDecompressWritesRaw()
    {
        var zipBytes = BuildZip(("inside.txt", "inside"));

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(zipBytes),
        });
        var client = BuildClient(out var service, out _, out _, handler);
        service.OnList = _ => new Internal.Twirp.ListArtifactsResponse
        {
            Artifacts =
            [
                new MonolithArtifact
                {
                    WorkflowRunBackendId = "run-1",
                    WorkflowJobRunBackendId = "job-1",
                    DatabaseId = 1,
                    Name = "name",
                    Size = zipBytes.Length,
                },
            ],
        };
        service.OnGetSignedUrl = _ => new GetSignedArtifactUrlResponse
        {
            SignedUrl = "https://blob.example.com/some-artifact.zip",
        };

        var response = await client.DownloadArtifactAsync(
            1, new DownloadArtifactOptions { Path = _root, SkipDecompress = true });

        Assert.Equal(_root, response.DownloadPath);
        var rawFile = Path.Combine(_root, "some-artifact.zip");
        Assert.True(File.Exists(rawFile));
        Assert.Equal(zipBytes, await File.ReadAllBytesAsync(rawFile));
    }

    [Fact(DisplayName = "FindBy routes through the public REST API")]
    public async Task FindByRoutesThroughRest()
    {
        var zipBytes = BuildZip(("a.txt", "a"));
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(zipBytes),
        });
        var client = BuildClient(out var service, out var publicApi, out _, handler);
        var findBy = new FindBy("token", 1, "owner", "repo");
        publicApi.OnDownload = (_, _) => new Uri("https://blob.example.com/sig");

        var response = await client.DownloadArtifactAsync(
            42, new DownloadArtifactOptions { Path = _root, FindBy = findBy });

        Assert.Equal(_root, response.DownloadPath);
        Assert.Empty(service.ListRequests);
        Assert.Single(publicApi.DownloadInvocations);
    }

    private static DefaultArtifactClient BuildClient(
        out FakeArtifactService service,
        out FakePublicArtifactsApi publicApi,
        out TestHttpClientFactory factory,
        TestHttpMessageHandler? handler = null)
    {
        service = new FakeArtifactService();
        publicApi = new FakePublicArtifactsApi();
        factory = new TestHttpClientFactory(handler ?? new TestHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK)));
        return new DefaultArtifactClient(service, new FakeBackendIdsProvider(), factory, publicApi);
    }

    private static byte[] BuildZip(params (string Name, string Content)[] entries)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                var entry = archive.CreateEntry(name);
                using var writer = new StreamWriter(entry.Open());
                writer.Write(content);
            }
        }

        return ms.ToArray();
    }
}
