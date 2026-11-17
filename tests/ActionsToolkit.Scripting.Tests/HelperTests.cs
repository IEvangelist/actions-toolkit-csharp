// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkit.Exec;
using ActionsToolkit.IO;

namespace ActionsToolkit.Scripting.Tests;

public sealed class HelperTests
{
    [Fact]
    public async Task ExecForwardsOutputOptionsAndNonzeroBehavior()
    {
        var command = OperatingSystem.IsWindows() ? "cmd" : "/bin/sh";
        string[] echo = OperatingSystem.IsWindows() ? ["/c", "echo", "facade"] : ["-c", "printf facade"];
        string[] fail = OperatingSystem.IsWindows() ? ["/c", "exit", "7"] : ["-c", "exit 7"];

        var output = await Exec.GetOutputAsync(command, echo, new ExecOptions { Silent = true });
        Assert.Equal(0, output.ExitCode);
        Assert.Contains("facade", output.Stdout, StringComparison.Ordinal);
        Assert.Equal(7, await Exec.RunAsync(command, fail, new ExecOptions { Silent = true, IgnoreReturnCode = true }));
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await Exec.RunAsync(command, fail, new ExecOptions { Silent = true }));
    }

    [Fact]
    public async Task ExecForwardsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await Exec.GetOutputAsync("dotnet", ["--info"], new ExecOptions { Silent = true }, cancellation.Token));
    }

    [Fact]
    public void IOAndGlobPreserveExistingOptionsAndExclusions()
    {
        using var directory = new TestDirectory();
        var source = directory.CreateFile("one.txt", "source");
        directory.CreateFile("skip.txt", "excluded");
        var folder = Path.Combine(directory.Root, "folder");
        IO.MakeDirectory(folder);
        var copied = Path.Combine(folder, "copy.txt");
        var moved = Path.Combine(folder, "moved.txt");

        IO.Copy(source, copied, new CopyOptions(Force: false));
        IO.Move(copied, moved, new MoveOptions(Force: false));
        Assert.Equal("source", File.ReadAllText(moved));
        Assert.Equal([source], Glob.Create(["*.txt"], ["skip.txt"]).GlobFiles(directory.Root).ToArray());
        Assert.True(Glob.Create("**/*.txt").Glob(directory.Root).HasMatches);
        IO.Remove(folder);
        Assert.False(Directory.Exists(folder));
        Assert.NotEmpty(IO.Which("dotnet"));
    }

    [Fact]
    public void GitHubRequiresExplicitAuthenticationAndReturnsOriginalContext()
    {
        Assert.Throws<ArgumentException>(() => GitHubApi.CreateClient(""));
        Assert.Throws<ArgumentException>(() => GitHubApi.CreateClient(" "));
        Assert.NotNull(GitHubApi.CreateClient("test-token-not-sent"));
        Assert.Same(global::ActionsToolkit.Octokit.Context.Current, GitHubApi.Context);
    }
}
