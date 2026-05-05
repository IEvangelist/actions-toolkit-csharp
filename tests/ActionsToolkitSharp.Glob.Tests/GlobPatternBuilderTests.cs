// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Glob.Tests;

public sealed class GlobPatternBuilderTests
{
    [Fact]
    public void GlobPatternBuilderThrowsWithoutIncludeOrExcludePatternsTest()
    {
        Assert.Throws<ArgumentException>(
            () => new DefaultGlobPatternResolverBuilder().Build());
    }

    [Fact]
    public void GlobPatternBuilderYieldsWorkingResolver()
    {
        var resolver = new DefaultGlobPatternResolverBuilder()
            .WithInclusions("**/*.md", "**/*.svg")
            .WithExclusions("*/more.md")
            .Build();

        var result = resolver.GetGlobResult("parent");
        Assert.True(result.HasMatches);
        Assert.Equal(5, result.Files.Count());

        string[] expectedFiles =
        [
            "parent/file.md",
            "parent/README.md",
            "parent/child/file.MD",
            "parent/child/assets/image.svg",
            "parent/child/grandchild/file.md",
        ];

        string[] expected =
        [
            .. expectedFiles.Select(
                static file => Path.GetFullPath(file))
        ];

        Assert.All(
            expected,
            expectedFile => Assert.Contains(
                expectedFile, result.Files.Select(f => f.FullName)));
    }
}
