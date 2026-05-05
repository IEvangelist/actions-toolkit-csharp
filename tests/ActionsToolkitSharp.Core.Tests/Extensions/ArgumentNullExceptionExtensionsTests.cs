// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Tests.Extensions;

public sealed class ArgumentNullExceptionExtensionsTests
{
    [Fact]
    public void ThrowIfNullExtensionCorrectlyCapturesParamNameTest()
    {
        object? pickles = default!;

        Assert.Throws<ArgumentNullException>(
            nameof(pickles), () => pickles!.ThrowIfNull());
    }

    [Fact]
    public void ThrowIfNullExtensionCorrectlyYieldsValueTest()
    {
        var pickles = new { Test = true };

        var result = pickles.ThrowIfNull();

        Assert.True(result.Test);
    }
}
