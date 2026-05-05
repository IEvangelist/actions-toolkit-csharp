// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Tests;

public sealed class NpmVersionTests
{
    [Theory]
    [InlineData("1.2.3", true)]
    [InlineData("v1.2.3", true)]
    [InlineData("1.2.3-alpha", true)]
    [InlineData("1.2.3-alpha.1", true)]
    [InlineData("1.2.3+build.1", true)]
    [InlineData("1.2", false)]
    [InlineData("1", false)]
    [InlineData("not-a-version", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("1.2.3.4", false)]
    public void TryParseHandlesVariousInputs(string? input, bool expected)
    {
        var actual = NpmVersion.TryParse(input, out _);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("1.2.3", "1.2.3")]
    [InlineData("v1.2.3", "1.2.3")]
    [InlineData("=1.2.3", "1.2.3")]
    [InlineData("1.2.3-beta", "1.2.3-beta")]
    [InlineData("1.2.3+build", "1.2.3")]
    [InlineData("not-a-version", null)]
    public void CleanReturnsCanonicalForm(string? input, string? expected)
    {
        Assert.Equal(expected, NpmVersion.Clean(input));
    }

    [Theory]
    [InlineData("1.2.3", true)]
    [InlineData("1.x", false)]
    [InlineData("^1.2.3", false)]
    [InlineData("~1.2.3", false)]
    [InlineData("*", false)]
    [InlineData("1.2", false)]
    public void IsExplicitReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, NpmVersion.IsExplicit(input));
    }

    [Theory]
    [InlineData("1.2.3", "1.2.3", 0)]
    [InlineData("1.2.4", "1.2.3", 1)]
    [InlineData("1.2.3", "1.2.4", -1)]
    [InlineData("2.0.0", "1.99.99", 1)]
    [InlineData("1.2.3-alpha", "1.2.3", -1)]
    [InlineData("1.2.3-alpha", "1.2.3-beta", -1)]
    [InlineData("1.2.3-alpha.1", "1.2.3-alpha.2", -1)]
    public void ComparesByPrecedence(string a, string b, int expectedSign)
    {
        Assert.True(NpmVersion.TryParse(a, out var va));
        Assert.True(NpmVersion.TryParse(b, out var vb));
        Assert.Equal(expectedSign, Math.Sign(va.CompareTo(vb)));
    }

    [Theory]
    [InlineData("1.2.3", "1.x", true)]
    [InlineData("1.2.3", "1.2.x", true)]
    [InlineData("1.2.3", "2.x", false)]
    [InlineData("1.2.3", "*", true)]
    [InlineData("1.2.3", "^1.2.0", true)]
    [InlineData("1.2.3", "^1.3.0", false)]
    [InlineData("1.2.3", "~1.2.0", true)]
    [InlineData("1.3.0", "~1.2.0", false)]
    [InlineData("1.2.3", ">=1.2.3", true)]
    [InlineData("1.2.2", ">=1.2.3", false)]
    [InlineData("1.2.3", ">1.2.3", false)]
    [InlineData("1.2.4", ">1.2.3", true)]
    [InlineData("1.2.3", "<=1.2.3", true)]
    [InlineData("1.2.4", "<=1.2.3", false)]
    [InlineData("1.5.0", "1.0.0 - 2.0.0", true)]
    [InlineData("3.0.0", "1.0.0 - 2.0.0", false)]
    [InlineData("1.5.0", "1.x || 2.x", true)]
    [InlineData("3.5.0", "1.x || 2.x", false)]
    [InlineData("1.2.3-alpha", "*", false)]
    public void NpmVersionRangeSatisfiesReturnsExpected(string version, string range, bool expected)
    {
        Assert.Equal(expected, NpmVersionRange.Satisfies(version, range));
    }
}
