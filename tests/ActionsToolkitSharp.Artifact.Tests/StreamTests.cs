// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Mirrors upstream
/// <c>actions/toolkit/packages/artifact/__tests__/stream.test.ts</c>.
/// Upstream <c>stream.test.ts</c> covers Node's
/// <c>stream.PassThrough</c>/buffering helpers we don't expose; this file
/// documents the parity gap and exercises the equivalent C# surface
/// (<see cref="ZipUploadStream"/>/<see cref="ZipUploadResult"/>) at the
/// <em>stream-shape</em> level. The upstream-specific behaviors covered here
/// are: the upload payload is a <strong>seekable</strong>,
/// <strong>readable</strong>, <strong>non-zero-length</strong> stream whose
/// <see cref="Stream.Position"/> is <strong>reset to zero</strong> before it
/// is handed to the caller. The Node <c>createRawFileUploadStream</c> error
/// propagation case has no public C# analogue: file-stream errors surface
/// directly out of <see cref="ZipUploadStream.CreateAsync"/> so they are
/// covered indirectly through <see cref="UploadArtifactTests"/>.
/// </summary>
public sealed class StreamTests : IDisposable
{
    private readonly string _root;

    public StreamTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "ats-stream-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            try
            {
                Directory.Delete(_root, recursive: true);
            }
            catch
            {
                // best effort cleanup
            }
        }
    }

    [Fact(DisplayName = "produced upload stream is seekable")]
    public async Task ProducedUploadStreamIsSeekable()
    {
        var result = await BuildAsync();
        try
        {
            Assert.True(result.Content.CanSeek);
        }
        finally
        {
            await result.Content.DisposeAsync();
        }
    }

    [Fact(DisplayName = "produced upload stream is readable")]
    public async Task ProducedUploadStreamIsReadable()
    {
        var result = await BuildAsync();
        try
        {
            Assert.True(result.Content.CanRead);
        }
        finally
        {
            await result.Content.DisposeAsync();
        }
    }

    [Fact(DisplayName = "produced upload stream is non-zero length")]
    public async Task ProducedUploadStreamHasNonZeroLength()
    {
        var result = await BuildAsync();
        try
        {
            Assert.True(result.UploadSize > 0);
            Assert.Equal(result.Content.Length, result.UploadSize);
        }
        finally
        {
            await result.Content.DisposeAsync();
        }
    }

    [Fact(DisplayName = "produced upload stream Position is reset to 0")]
    public async Task ProducedUploadStreamPositionIsZero()
    {
        var result = await BuildAsync();
        try
        {
            Assert.Equal(0L, result.Content.Position);
        }
        finally
        {
            await result.Content.DisposeAsync();
        }
    }

    [Fact(DisplayName = "produced upload stream can be re-read after seeking back to 0")]
    public async Task ProducedUploadStreamCanBeRereadFromZero()
    {
        var result = await BuildAsync();
        try
        {
            var firstPass = await ReadAllAsync(result.Content);
            result.Content.Position = 0;
            var secondPass = await ReadAllAsync(result.Content);

            Assert.Equal(firstPass.Length, secondPass.Length);
            Assert.Equal(firstPass, secondPass);
        }
        finally
        {
            await result.Content.DisposeAsync();
        }
    }

    private async Task<ZipUploadResult> BuildAsync()
    {
        var file = Path.Combine(_root, "stream-test.txt");
        await File.WriteAllTextAsync(file, "hello stream test");

        var spec = UploadZipSpecification.GetUploadZipSpecification([file], _root);
        return await ZipUploadStream.CreateAsync(spec, compressionLevel: null, CancellationToken.None);
    }

    private static async Task<byte[]> ReadAllAsync(Stream stream)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        return buffer.ToArray();
    }
}
