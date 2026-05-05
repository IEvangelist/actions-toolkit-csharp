// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography;

namespace ActionsToolkit.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/upload-artifact.test.ts</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class UploadArtifactTests : IDisposable
{
    private readonly string _root;
    private readonly EnvironmentScope _serverScope;

    public UploadArtifactTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "actions-toolkit-upload-artifact-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        _serverScope = new EnvironmentScope("GITHUB_SERVER_URL", "https://github.com");
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

        var client = BuildClient(out _, out _);

        await Assert.ThrowsAsync<GhesNotSupportedException>(
            () => client.UploadArtifactAsync("name", [], _root, options: null).AsTask());
    }

    [Fact(DisplayName = "Throws InvalidArtifactNameException for an invalid name")]
    public async Task ThrowsForInvalidName()
    {
        var client = BuildClient(out _, out _);

        await Assert.ThrowsAsync<InvalidArtifactNameException>(
            () => client.UploadArtifactAsync("bad/name", [], _root, options: null).AsTask());
    }

    [Fact(DisplayName = "Throws FilesNotFoundException when no files match")]
    public async Task ThrowsWhenNoFiles()
    {
        var client = BuildClient(out _, out _);

        await Assert.ThrowsAsync<FilesNotFoundException>(
            () => client.UploadArtifactAsync("name", [], _root, options: null).AsTask());
    }

    [Fact(DisplayName = "Throws InvalidArtifactResponseException when CreateArtifact returns ok=false")]
    public async Task ThrowsWhenCreateNotOk()
    {
        var fileA = Path.Combine(_root, "a.txt");
        await File.WriteAllTextAsync(fileA, "x");

        var client = BuildClient(out var service, out _);
        service.OnCreate = _ => new CreateArtifactResponse { Ok = false, SignedUploadUrl = string.Empty };

        await Assert.ThrowsAsync<InvalidArtifactResponseException>(
            () => client.UploadArtifactAsync("name", [fileA], _root, options: null).AsTask());
    }

    [Fact(DisplayName = "Issues create + PUT + finalize and returns id, size, digest on success")]
    public async Task UploadsAndFinalizes()
    {
        var fileA = Path.Combine(_root, "a.txt");
        await File.WriteAllTextAsync(fileA, "hello");

        var blobUploads = new List<byte[]>();
        var handler = new TestHttpMessageHandler(req =>
        {
            using var stream = new MemoryStream();
            req.Content!.ReadAsStream().CopyTo(stream);
            blobUploads.Add(stream.ToArray());
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        var client = BuildClient(out var service, out _, handler);
        service.OnCreate = _ => new CreateArtifactResponse
        {
            Ok = true,
            SignedUploadUrl = "https://blob.example.com/sig",
        };
        service.OnFinalize = _ => new FinalizeArtifactResponse { Ok = true, ArtifactId = 4242 };

        var response = await client.UploadArtifactAsync("name", [fileA], _root, options: null);

        Assert.Equal(4242, response.Id);
        Assert.True(response.Size > 0);
        Assert.NotNull(response.Digest);
        Assert.Single(handler.Requests);
        Assert.Single(blobUploads);

        var put = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Put, put.Method);
        Assert.Equal("https://blob.example.com/sig", put.RequestUri!.ToString());
        Assert.True(put.Headers.TryGetValues("x-ms-blob-type", out var blobTypes));
        Assert.Equal("BlockBlob", blobTypes.Single());

        var finalize = Assert.Single(service.FinalizeRequests);
        Assert.Equal(response.Size, finalize.Size);
        Assert.StartsWith("sha256:", finalize.Hash, StringComparison.Ordinal);

        var expectedDigest = Convert.ToHexStringLower(SHA256.HashData(blobUploads[0]));
        Assert.Equal($"sha256:{expectedDigest}", finalize.Hash);
    }

    [Fact(DisplayName = "Throws ArtifactUploadException when the blob PUT fails")]
    public async Task ThrowsWhenBlobUploadFails()
    {
        var fileA = Path.Combine(_root, "a.txt");
        await File.WriteAllTextAsync(fileA, "x");

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var client = BuildClient(out var service, out _, handler);
        service.OnCreate = _ => new CreateArtifactResponse
        {
            Ok = true,
            SignedUploadUrl = "https://blob.example.com/sig",
        };

        await Assert.ThrowsAsync<ArtifactUploadException>(
            () => client.UploadArtifactAsync("name", [fileA], _root, options: null).AsTask());
    }

    [Fact(DisplayName = "Propagates RetentionDays into the CreateArtifact request")]
    public async Task PropagatesRetentionDays()
    {
        var fileA = Path.Combine(_root, "a.txt");
        await File.WriteAllTextAsync(fileA, "x");

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Created));

        var client = BuildClient(out var service, out _, handler);
        service.OnCreate = _ => new CreateArtifactResponse
        {
            Ok = true,
            SignedUploadUrl = "https://blob.example.com/sig",
        };
        service.OnFinalize = _ => new FinalizeArtifactResponse { Ok = true, ArtifactId = 1 };

        await client.UploadArtifactAsync(
            "name",
            [fileA],
            _root,
            new UploadArtifactOptions { RetentionDays = 5 });

        var request = Assert.Single(service.CreateRequests);
        Assert.NotNull(request.ExpiresAt);
        Assert.InRange(
            request.ExpiresAt!.Value - DateTimeOffset.UtcNow,
            TimeSpan.FromDays(4.99),
            TimeSpan.FromDays(5.01));
    }

    private static DefaultArtifactClient BuildClient(
        out FakeArtifactService service,
        out FakePublicArtifactsApi publicApi,
        TestHttpMessageHandler? handler = null)
    {
        service = new FakeArtifactService();
        publicApi = new FakePublicArtifactsApi();
        var factory = new TestHttpClientFactory(handler ?? new TestHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK)));
        return new DefaultArtifactClient(service, new FakeBackendIdsProvider(), factory, publicApi);
    }
}
