// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/upload-zip-specification.test.ts</c>:
/// validates root-directory and file-list invariants enforced by
/// <see cref="UploadZipSpecification"/>.
/// </summary>
public sealed class UploadZipSpecificationTests : IDisposable
{
    private readonly string _root;

    public UploadZipSpecificationTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "ats-upload-zip-spec-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
        }
    }

    [Fact(DisplayName = "ValidateRootDirectory throws when the path does not exist")]
    public void ValidateRootDirectoryThrowsWhenMissing()
    {
        var missing = Path.Combine(_root, "missing");

        Assert.Throws<DirectoryNotFoundException>(
            () => UploadZipSpecification.ValidateRootDirectory(missing));
    }

    [Fact(DisplayName = "ValidateRootDirectory throws when the path is a file")]
    public void ValidateRootDirectoryThrowsWhenFile()
    {
        var file = Path.Combine(_root, "f.txt");
        File.WriteAllText(file, "x");

        Assert.Throws<InvalidOperationException>(
            () => UploadZipSpecification.ValidateRootDirectory(file));
    }

    [Fact(DisplayName = "ValidateRootDirectory accepts an existing directory")]
    public void ValidateRootDirectoryAcceptsDirectory() =>
        UploadZipSpecification.ValidateRootDirectory(_root);

    [Fact(DisplayName = "GetUploadZipSpecification computes destination paths relative to root")]
    public void ComputesRelativeDestinationPaths()
    {
        var fileA = Path.Combine(_root, "a.txt");
        var nested = Path.Combine(_root, "nested");
        Directory.CreateDirectory(nested);
        var fileB = Path.Combine(nested, "b.txt");
        File.WriteAllText(fileA, "a");
        File.WriteAllText(fileB, "b");

        var spec = UploadZipSpecification.GetUploadZipSpecification([fileA, fileB], _root);

        Assert.Collection(
            spec,
            entry =>
            {
                Assert.Equal(fileA, entry.SourcePath);
                Assert.Equal(Path.DirectorySeparatorChar + "a.txt", entry.DestinationPath);
            },
            entry =>
            {
                Assert.Equal(fileB, entry.SourcePath);
                Assert.Equal(
                    Path.DirectorySeparatorChar + "nested" + Path.DirectorySeparatorChar + "b.txt",
                    entry.DestinationPath);
            });
    }

    [Fact(DisplayName = "GetUploadZipSpecification rejects files outside the root directory")]
    public void RejectsFilesOutsideRoot()
    {
        var outside = Path.Combine(Path.GetTempPath(), "ats-upload-zip-spec-outside-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(outside, "x");
        try
        {
            Assert.Throws<InvalidOperationException>(
                () => UploadZipSpecification.GetUploadZipSpecification([outside], _root));
        }
        finally
        {
            try { File.Delete(outside); } catch { /* best effort */ }
        }
    }

    [Fact(DisplayName = "GetUploadZipSpecification throws when a file is missing")]
    public void ThrowsWhenFileMissing()
    {
        var missing = Path.Combine(_root, "missing.txt");

        Assert.Throws<FileNotFoundException>(
            () => UploadZipSpecification.GetUploadZipSpecification([missing], _root));
    }

    [Fact(DisplayName = "GetUploadZipSpecification produces a directory entry for empty directories")]
    public void ProducesDirectoryEntryForEmptyDirectories()
    {
        var emptyDir = Path.Combine(_root, "empty");
        Directory.CreateDirectory(emptyDir);

        var spec = UploadZipSpecification.GetUploadZipSpecification([emptyDir], _root);

        var entry = Assert.Single(spec);
        Assert.Null(entry.SourcePath);
        Assert.Equal(Path.DirectorySeparatorChar + "empty", entry.DestinationPath);
    }
}
