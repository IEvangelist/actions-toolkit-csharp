// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Mirrors upstream <c>__tests__/path-and-artifact-name-validation.test.ts</c>.
/// </summary>
public sealed class PathAndArtifactNameValidationTests
{
    [Fact(DisplayName = "Accepts a valid artifact name")]
    public void AcceptsValidArtifactName() =>
        PathAndArtifactNameValidation.ValidateArtifactName("valid-name_1");

    [Theory(DisplayName = "Rejects artifact names containing illegal characters")]
    [InlineData("bad/name")]
    [InlineData("bad\\name")]
    [InlineData("bad:name")]
    [InlineData("bad?name")]
    [InlineData("bad*name")]
    [InlineData("bad\"name")]
    [InlineData("bad<name")]
    [InlineData("bad>name")]
    [InlineData("bad|name")]
    public void RejectsArtifactNamesWithIllegalCharacters(string name) =>
        Assert.Throws<InvalidArtifactNameException>(
            () => PathAndArtifactNameValidation.ValidateArtifactName(name));

    [Fact(DisplayName = "Rejects an empty artifact name")]
    public void RejectsEmptyArtifactName() =>
        Assert.Throws<InvalidArtifactNameException>(
            () => PathAndArtifactNameValidation.ValidateArtifactName(string.Empty));

    [Fact(DisplayName = "Accepts a valid file path with separators")]
    public void AcceptsValidFilePath() =>
        PathAndArtifactNameValidation.ValidateFilePath("/foo/bar/baz.txt");

    [Theory(DisplayName = "Rejects file paths containing illegal characters")]
    [InlineData("/foo|bar.txt")]
    [InlineData("/foo:bar.txt")]
    [InlineData("/foo*bar.txt")]
    [InlineData("/foo?bar.txt")]
    [InlineData("/foo<bar.txt")]
    [InlineData("/foo>bar.txt")]
    [InlineData("/foo\"bar.txt")]
    public void RejectsFilePathWithIllegalCharacters(string path) =>
        Assert.Throws<InvalidArtifactNameException>(
            () => PathAndArtifactNameValidation.ValidateFilePath(path));

    [Fact(DisplayName = "Rejects an empty file path")]
    public void RejectsEmptyFilePath() =>
        Assert.Throws<InvalidArtifactNameException>(
            () => PathAndArtifactNameValidation.ValidateFilePath(string.Empty));
}
