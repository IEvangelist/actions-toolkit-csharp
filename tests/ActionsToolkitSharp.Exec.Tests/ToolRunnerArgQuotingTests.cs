// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Exec.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/exec/__tests__/exec.test.ts"/>.
/// Validates upstream <c>argStringToArray</c> behavior — the runner's parser for splitting an
/// arbitrary command-line string into argv tokens. Mirrors the cmd/exec quoting concerns that
/// the upstream <c>execs .exe with arg quoting</c> and <c>argStringToArray</c> tests cover.
/// </summary>
public sealed class ToolRunnerArgQuotingTests
{
    [Fact(DisplayName = "argStringToArray splits unquoted tokens on whitespace")]
    public void ArgStringSplitsUnquotedTokensOnWhitespace()
    {
        var actual = ArgString.ToArray("dotnet build --no-restore");
        Assert.Equal(["dotnet", "build", "--no-restore"], actual);
    }

    [Fact(DisplayName = "argStringToArray honors a quoted span containing spaces")]
    public void ArgStringHonorsQuotedSpanContainingSpaces()
    {
        var actual = ArgString.ToArray("\"my tool\" arg1 \"two words\"");
        Assert.Equal(["my tool", "arg1", "two words"], actual);
    }

    [Fact(DisplayName = "argStringToArray un-escapes embedded backslash-quote inside quoted spans")]
    public void ArgStringUnescapesEmbeddedBackslashQuoteInsideQuotedSpans()
    {
        var actual = ArgString.ToArray("tool \"a\\\"b\" trailing");
        Assert.Equal(["tool", "a\"b", "trailing"], actual);
    }

    [Fact(DisplayName = "argStringToArray returns empty array for empty input")]
    public void ArgStringReturnsEmptyArrayForEmptyInput()
    {
        Assert.Empty(ArgString.ToArray(""));
    }

    [Fact(DisplayName = "argStringToArray collapses leading and trailing whitespace")]
    public void ArgStringCollapsesLeadingAndTrailingWhitespace()
    {
        var actual = ArgString.ToArray("   tool    arg   ");
        Assert.Equal(["tool", "arg"], actual);
    }

    [Theory(DisplayName = "argStringToArray preserves shell metacharacters inside quotes")]
    [InlineData("tool \"a&b\" \"c|d\" \"e;f\" \"g<h\"", new[] { "tool", "a&b", "c|d", "e;f", "g<h" })]
    public void ArgStringPreservesShellMetacharactersInsideQuotes(string input, string[] expected)
    {
        Assert.Equal(expected, ArgString.ToArray(input));
    }

    [Fact(DisplayName = "argStringToArray handles literal backslashes outside quotes verbatim")]
    public void ArgStringHandlesLiteralBackslashesOutsideQuotesVerbatim()
    {
        // Outside quotes, backslashes are literal (only `\"` inside quotes is special).
        var actual = ArgString.ToArray(@"C:\path\to\tool arg");
        Assert.Equal([@"C:\path\to\tool", "arg"], actual);
    }
}
