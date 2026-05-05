// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace ActionsToolkit.IO.Tests;

[SuppressMessage(
    "Performance",
    "CA1859:Use concrete types when possible for improved performance",
    Justification = "We need to use the IOperations interface.")]
public sealed class OperationTests(TempFolderTestFixture fixture) : IClassFixture<TempFolderTestFixture>
{
    [Fact]
    public void CopiesFilesWithNoFlags()
    {
        // Arrange
        var root = Path.Combine(fixture.TempFolder, "copy_with_no_flags");
        var sourceFile = Path.Combine(root, "cp_source");
        var targetFile = Path.Combine(root, "cp_target");
        IOperations operations = new Operations();
        operations.MakeDirectory(root);
        File.WriteAllText(sourceFile, "test file content");

        // Act
        operations.Copy(sourceFile, targetFile);

        // Assert
        Assert.Equal("test file content", File.ReadAllText(targetFile));
    }

    [Fact]
    public void CopiesFilesUsingForce()
    {
        // Arrange
        var root = Path.Combine(fixture.TempFolder, "copy_with_-f");
        var sourceFile = Path.Combine(root, "cp_source");
        var targetFile = Path.Combine(root, "cp_target");
        IOperations operations = new Operations();
        operations.MakeDirectory(root);
        File.WriteAllText(sourceFile, "test file content");

        // Act
        operations.Copy(sourceFile, targetFile);

        // Assert
        Assert.Equal("test file content", File.ReadAllText(targetFile));
    }

    [Fact]
    public void CopiesFileIntoDirectory()
    {
        // Arrange
        var root = Path.Combine(fixture.TempFolder, "copy_file_to_directory");
        var sourceFile = Path.Combine(root, "cp_source");
        var targetDirectory = Path.Combine(root, "cp_target");
        var targetFile = Path.Combine(targetDirectory, "cp_source");
        IOperations operations = new Operations();
        operations.MakeDirectory(targetDirectory);
        File.WriteAllText(sourceFile, "test file content");

        // Act
        operations.Copy(sourceFile, targetFile,
            new(Recursive: false, Force: true));

        // Assert
        Assert.Equal("test file content", File.ReadAllText(targetFile));
    }

    [Fact]
    public void TryCopyingToExistingFileWithFlagN()
    {
        // Arrange
        var root = Path.Combine(fixture.TempFolder, "copy_to_existing");
        var sourceFile = Path.Combine(root, "cp_source");
        var targetFile = Path.Combine(root, "cp_target");
        IOperations operations = new Operations();
        operations.MakeDirectory(root);
        File.WriteAllText(sourceFile, "test file content");
        File.WriteAllText(targetFile, "correct content");

        // Act
        operations.Copy(sourceFile, targetFile,
            new(Recursive: false, Force: false));

        // Assert
        Assert.Equal("correct content", File.ReadAllText(targetFile));
    }

    [Fact]
    public void CopiesDirectoryIntoExistingDestinationWithFlagR()
    {
        // Arrange
        var root = Path.Combine(fixture.TempFolder, "cp_with_-r_existing_dest");
        var sourceFolder = Path.Combine(root, "cp_source");
        var sourceFile = Path.Combine(sourceFolder, "cp_source_file");
        var targetFolder = Path.Combine(root, "cp_target");
        var targetFile = Path.Combine(targetFolder, "cp_source", "cp_source_file");
        IOperations operations = new Operations();
        operations.MakeDirectory(sourceFolder);
        File.WriteAllText(sourceFile, "test file content");
        operations.MakeDirectory(targetFolder);

        // Act
        operations.Copy(sourceFolder, targetFolder,
            new(Recursive: true));

        // Assert
        Assert.Equal("test file content", File.ReadAllText(targetFile));
    }

    [Fact]
    public void CopiesDirectoryIntoExistingDestinationWithRecursionAndWithoutCopyingSourceDir()
    {
        // Arrange
        var root = Path.Combine(fixture.TempFolder, "cp_with_-r_existing_dest_no_source_dir");
        var sourceFolder = Path.Combine(root, "cp_source");
        var sourceFile = Path.Combine(sourceFolder, "cp_source_file");
        var targetFolder = Path.Combine(root, "cp_target");
        var targetFile = Path.Combine(targetFolder, "cp_source_file");
        IOperations operations = new Operations();
        operations.MakeDirectory(sourceFolder);
        File.WriteAllText(sourceFile, "test file content");
        operations.MakeDirectory(targetFolder);

        // Act
        operations.Copy(sourceFolder, targetFolder,
            new(Recursive: true, CopySourceDirectory: false));

        // Assert
        Assert.Equal("test file content", File.ReadAllText(targetFile));
    }

    [Fact]
    public void CopiesDirectoryIntoNonExistingDestinationWithFlagR()
    {
        // Arrange
        var root = Path.Combine(fixture.TempFolder, "cp_with_-r_nonexistent_dest");
        var sourceFolder = Path.Combine(root, "cp_source");
        var sourceFile = Path.Combine(sourceFolder, "cp_source_file");

        var targetFolder = Path.Combine(root, "cp_target");
        var targetFile = Path.Combine(targetFolder, "cp_source_file");
        IOperations operations = new Operations();
        operations.MakeDirectory(sourceFolder);
        File.WriteAllText(sourceFile, "test file content");

        // Act
        operations.Copy(sourceFolder, targetFolder,
            new(Recursive: true, CopySourceDirectory: false));

        // Assert
        Assert.Equal("test file content", File.ReadAllText(targetFile));
    }

    [Fact]
    public void TriesToCopyDirectoryWithoutFlagR()
    {
        // Arrange
        var root = Path.Combine(fixture.TempFolder, "cp_without_-r");
        var sourceFolder = Path.Combine(root, "cp_source");
        var sourceFile = Path.Combine(sourceFolder, "cp_source_file");

        var targetFolder = Path.Combine(root, "cp_target");
        var targetFile = Path.Combine(targetFolder, "cp_source", "cp_source_file");
        IOperations operations = new Operations();
        operations.MakeDirectory(sourceFolder);
        File.WriteAllText(sourceFile, "test file content");

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() =>
            operations.Copy(
                sourceFolder, targetFolder, new(Recursive: false)));

        Assert.False(File.Exists(targetFile));
    }
}
