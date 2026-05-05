// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.IO.Tests;

public sealed class CopyOptionsTests
{
    [Fact]
    public void CopyOptionsCorrectlyDefaultsProperties()
    {
        CopyOptions options = default;

        Assert.False(options.Recursive);
        Assert.False(options.Force);
        Assert.False(options.CopySourceDirectory);

        options = new();

        Assert.False(options.Recursive);
        Assert.True(options.Force);
        Assert.True(options.CopySourceDirectory);
    }
}
