// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Tests.Extensions;

public sealed class ObjectExtensionsTests
{
    [Fact]
    public void ToCommandPropertiesCorrectlyCreatesSelfNamedKeyValuePairTest()
    {
        var key = "Value of 1, 2, 3...";
        var actual = key.ToCommandProperties();
        Assert.Equal("Value of 1, 2, 3...", actual[nameof(key)]);
    }
}
