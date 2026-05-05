// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Tests.Services;

public sealed class DefaultWorkflowStepServiceTests
{
    [Fact]
    public async Task DefaultWorkflowStepServiceAddsPathIssuesTest()
    {
        Environment.SetEnvironmentVariable(Keys.GITHUB_PATH, null);

        var testConsole = new TestConsole();
        ICommandIssuer commandIssuer = new DefaultCommandIssuer(testConsole);
        IFileCommandIssuer fileCommandIssuer = new DefaultFileCommandIssuer(
            (filePath, actual) => ValueTask.CompletedTask);

        var sut = new DefaultCoreService(
            testConsole, commandIssuer, fileCommandIssuer);

        await sut.AddPathAsync("some/path/to/test");

        Assert.Equal(
            expected: $"""
                ::add-path::some/path/to/test{Environment.NewLine}
                """,
            actual: testConsole.Output.ToString());
    }

    [Fact]
    public async Task DefaultWorkflowStepServiceAddsPathCorrectlyTest()
    {
        var path = "test-001.txt";
        Environment.SetEnvironmentVariable(Keys.GITHUB_PATH, path);
        await File.WriteAllTextAsync(path, "");

        var testConsole = new TestConsole();
        ICommandIssuer commandIssuer = new DefaultCommandIssuer(testConsole);
        IFileCommandIssuer fileCommandIssuer = new DefaultFileCommandIssuer(
            (filePath, actual) =>
            {
                Assert.Equal("path/to/test", actual);

                return ValueTask.CompletedTask;
            });

        var sut = new DefaultCoreService(
            testConsole, commandIssuer, fileCommandIssuer);

        await sut.AddPathAsync("path/to/test");
    }
}
