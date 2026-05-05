// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Tests;

public sealed class HttpErrorTests
{
    [Fact(DisplayName = "captures status code")]
    public void CapturesStatusCode()
    {
        var err = new HttpError(System.Net.HttpStatusCode.NotFound);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, err.HttpStatusCode);
        Assert.Contains("404", err.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "tolerates null status")]
    public void ToleratesNullStatus()
    {
        var err = new HttpError(null);
        Assert.Null(err.HttpStatusCode);
        Assert.NotNull(err.Message);
    }
}
