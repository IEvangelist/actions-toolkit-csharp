// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Exec.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/exec/__tests__/exec.test.ts"/>.
/// Mirrors the upstream <c>Handles output callbacks</c>, <c>Handles large stdline</c>, and
/// <c>correctly outputs for getExecOutput with additional listeners</c> concerns by exercising
/// the <see cref="ExecListeners"/> stdout/stderr/stdline/errline/debug delegates.
/// </summary>
public sealed class ListenersTests
{
    [Fact(DisplayName = "Handles output callbacks")]
    public async Task HandlesOutputCallbacks()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.EchoCommand("hello-from-listener");

        var stdoutChunks = new List<string>();
        var options = new ExecOptions
        {
            Silent = true,
            Listeners = new ExecListeners
            {
                Stdout = data => stdoutChunks.Add(Encoding.UTF8.GetString(data.Span)),
            },
        };

        var exitCode = await exec.ExecAsync(commandLine, args, options);

        Assert.Equal(0, exitCode);
        Assert.Contains("hello-from-listener", string.Concat(stdoutChunks), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Handles large stdline")]
    public async Task HandlesLargeStdline()
    {
        var exec = BuildExec();
        // Build a single-line payload around 32 KiB to ensure multi-chunk buffering still emits exactly one stdline.
        // We stage the payload in a temp file and print it back via type/cat so we don't hit
        // platform command-line length limits (Windows cmd.exe caps near 8 KiB).
        var payload = new string('x', 32 * 1024);
        var tempFile = Path.Combine(Path.GetTempPath(), $"ats-exec-large-{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(tempFile, payload);
        try
        {
            string commandLine;
            string[] args;
            if (TestPlatform.IsWindows)
            {
                commandLine = "cmd";
                args = ["/c", "type", tempFile];
            }
            else
            {
                commandLine = "/bin/sh";
                args = ["-c", $"cat '{tempFile}'"];
            }

            string? captured = null;
            var options = new ExecOptions
            {
                Silent = true,
                Listeners = new ExecListeners
                {
                    Stdline = line => captured ??= line,
                },
            };

            var exitCode = await exec.ExecAsync(commandLine, args, options);

            Assert.Equal(0, exitCode);
            Assert.NotNull(captured);
            Assert.Contains(payload, captured!, StringComparison.Ordinal);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { /* best effort */ }
        }
    }

    [Fact(DisplayName = "correctly outputs for getExecOutput")]
    public async Task CorrectlyOutputsForGetExecOutput()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.EchoCommand("captured-stdout");

        var output = await exec.GetExecOutputAsync(commandLine, args, new ExecOptions { Silent = true });

        Assert.Equal(0, output.ExitCode);
        Assert.Contains("captured-stdout", output.Stdout, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "correctly outputs for getExecOutput with additional listeners")]
    public async Task CorrectlyOutputsForGetExecOutputWithAdditionalListeners()
    {
        var exec = BuildExec();
        var (commandLine, args) = TestPlatform.EchoCommand("listener-and-output");

        var listenerCapture = new StringBuilder();
        var options = new ExecOptions
        {
            Silent = true,
            Listeners = new ExecListeners
            {
                Stdout = data => listenerCapture.Append(Encoding.UTF8.GetString(data.Span)),
            },
        };

        var output = await exec.GetExecOutputAsync(commandLine, args, options);

        Assert.Equal(0, output.ExitCode);
        Assert.Contains("listener-and-output", output.Stdout, StringComparison.Ordinal);
        Assert.Contains("listener-and-output", listenerCapture.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "correctly outputs for getExecOutput when total size exceeds buffer size")]
    public async Task CorrectlyOutputsForGetExecOutputWhenTotalSizeExceedsBufferSize()
    {
        var exec = BuildExec();
        // The runner's pump uses a 4 KiB read buffer; emit ~1 MiB so we are guaranteed multiple chunks.
        // The payload is staged via a temp file to dodge OS command-line length caps.
        var payload = new string('a', 1024 * 1024);
        var tempFile = Path.Combine(Path.GetTempPath(), $"ats-exec-1mb-{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(tempFile, payload);
        try
        {
            string commandLine;
            string[] args;
            if (TestPlatform.IsWindows)
            {
                commandLine = "cmd";
                args = ["/c", "type", tempFile];
            }
            else
            {
                commandLine = "/bin/sh";
                args = ["-c", $"cat '{tempFile}'"];
            }

            var output = await exec.GetExecOutputAsync(commandLine, args, new ExecOptions { Silent = true });

            Assert.Equal(0, output.ExitCode);
            Assert.True(output.Stdout.Length >= payload.Length, $"expected captured stdout to be >= payload size, was {output.Stdout.Length}");
        }
        finally
        {
            try { File.Delete(tempFile); } catch { /* best effort */ }
        }
    }

    private static IExecService BuildExec()
    {
        return new ServiceCollection()
            .AddGitHubActionsExec()
            .BuildServiceProvider()
            .GetRequiredService<IExecService>();
    }
}
