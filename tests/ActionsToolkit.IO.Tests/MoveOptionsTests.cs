// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.IO.Tests;

public sealed class MoveOptionsTests
{
    [Fact]
    public void MoveOptionsCorrectlyDefaultsProperties()
    {
        MoveOptions options = default;

        Assert.False(options.Force);

        options = new();

        Assert.True(options.Force);
    }
}
