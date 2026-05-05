// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void ToBase64ReturnsBase64EncodedString()
    {
        // Arrange
        var input = "Hello, World!";

        // Act
        var result = StringExtensions.ToBase64(input);

        // Assert
        Assert.Equal("SGVsbG8sIFdvcmxkIQ==", result);
    }

    [Fact]
    public void FromBase64ReturnsDecodedString()
    {
        // Arrange
        var input = "SGVsbG8sIFdvcmxkIQ==";

        // Act
        var result = StringExtensions.FromBase64(input);

        // Assert
        Assert.Equal("Hello, World!", result);
    }
}
