// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient.Tests;

public class HttpMethodExtensionsTests
{
    [Theory]
    [InlineData("UNDEFINED", false)]
    [InlineData("HEAD", true)]
    [InlineData("OPTIONS", true)]
    [InlineData("DELETE", true)]
    [InlineData("GET", true)]
    [InlineData("PUT", false)]
    [InlineData("POST", false)]
    [InlineData("PATCH", false)]
    public void IsRetriableMethodCorrectlyEvaluatesEligibility(string method, bool expected)
    {
        var httpMethod = new HttpMethod(method);

        var actual = httpMethod.IsRetriableMethod();

        Assert.Equal(expected, actual);
    }
}
