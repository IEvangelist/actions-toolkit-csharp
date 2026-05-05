// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace ActionsToolkitSharp.Exec;

/// <summary>
/// Cross-platform process runner. C# port of the upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/exec/src/toolrunner.ts">
/// <c>toolrunner.ts</c></a> from <c>@actions/exec</c> (3.0.0). Handles command-line
/// quoting, argument parsing, line-buffered output dispatch, stdin streaming, and
/// exit-code propagation.
/// </summary>
internal sealed class ToolRunner
{
    private static readonly bool s_isWindows =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private string _toolPath;
    private readonly string[] _args;
    private readonly ExecOptions _options;

    public ToolRunner(string toolPath, string[]? args, ExecOptions? options)
    {
        if (string.IsNullOrEmpty(toolPath))
        {
            throw new ArgumentException("Parameter 'toolPath' cannot be null or empty.", nameof(toolPath));
        }

        _toolPath = toolPath;
        _args = args ?? [];
        _options = options ?? new ExecOptions();
    }

    private void Debug(string message)
    {
        _options.Listeners?.Debug?.Invoke(message);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the resolved tool path ends with <c>.cmd</c>
    /// or <c>.bat</c> (case-insensitive).
    /// </summary>
    private bool IsCmdFile()
    {
        var upper = _toolPath.ToUpperInvariant();
        return upper.EndsWith(".CMD", StringComparison.Ordinal) ||
               upper.EndsWith(".BAT", StringComparison.Ordinal);
    }

    /// <summary>
    /// Computes the file name to pass to <see cref="Process"/>. On Windows for a
    /// <c>.cmd</c>/<c>.bat</c> tool this is <c>%COMSPEC%</c> (or <c>cmd.exe</c>).
    /// </summary>
    private string GetSpawnFileName()
    {
        if (s_isWindows && IsCmdFile())
        {
            return Environment.GetEnvironmentVariable("COMSPEC") ?? "cmd.exe";
        }

        return _toolPath;
    }

    /// <summary>
    /// Computes the argv passed to <see cref="Process"/>. On Windows for a
    /// <c>.cmd</c>/<c>.bat</c> tool the args are folded into a single
    /// <c>/D /S /C "..."</c> argument string per upstream behavior.
    /// </summary>
    private string[] GetSpawnArgs(ExecOptions options)
    {
        if (s_isWindows && IsCmdFile())
        {
            var argline = new StringBuilder();
            argline.Append("/D /S /C \"").Append(WindowsQuoteCmdArg(_toolPath));
            foreach (var a in _args)
            {
                argline.Append(' ');
                argline.Append(options.WindowsVerbatimArguments ? a : WindowsQuoteCmdArg(a));
            }
            argline.Append('"');
            return [argline.ToString()];
        }

        return _args;
    }

    /// <summary>
    /// Builds the <c>[command]…</c> echo string written to <c>OutStream</c> at the
    /// start of every successful exec.
    /// </summary>
    private string GetCommandString(ExecOptions options, bool noPrefix = false)
    {
        var toolPath = GetSpawnFileName();
        var args = GetSpawnArgs(options);
        var cmd = new StringBuilder(noPrefix ? "" : "[command]");

        if (s_isWindows)
        {
            if (IsCmdFile())
            {
                cmd.Append(toolPath);
                foreach (var a in args)
                {
                    cmd.Append(' ').Append(a);
                }
            }
            else if (options.WindowsVerbatimArguments)
            {
                cmd.Append('"').Append(toolPath).Append('"');
                foreach (var a in args)
                {
                    cmd.Append(' ').Append(a);
                }
            }
            else
            {
                cmd.Append(WindowsQuoteCmdArg(toolPath));
                foreach (var a in args)
                {
                    cmd.Append(' ').Append(WindowsQuoteCmdArg(a));
                }
            }
        }
        else
        {
            cmd.Append(toolPath);
            foreach (var a in args)
            {
                cmd.Append(' ').Append(a);
            }
        }

        return cmd.ToString();
    }

    /// <summary>
    /// Applies Windows quoting rules. For <c>.cmd</c>/<c>.bat</c> the cmd.exe-specific
    /// rules are used; for everything else libuv's <c>quote_cmd_arg</c> rules are used.
    /// </summary>
    [SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "StringBuilder fluent calls return this.")]
    public string WindowsQuoteCmdArg(string arg)
    {
        if (!IsCmdFile())
        {
            return UvQuoteCmdArg(arg);
        }

        if (string.IsNullOrEmpty(arg))
        {
            return "\"\"";
        }

        // cmd.exe special characters that force quoting.
        ReadOnlySpan<char> cmdSpecialChars = [' ', '\t', '&', '(', ')', '[', ']', '{', '}', '^', '=', ';', '!', '\'', '+', ',', '`', '~', '|', '<', '>', '"'];
        var needsQuotes = false;
        foreach (var c in arg)
        {
            if (cmdSpecialChars.IndexOf(c) >= 0)
            {
                needsQuotes = true;
                break;
            }
        }

        if (!needsQuotes)
        {
            return arg;
        }

        // Walk the string in reverse, doubling backslashes immediately before quotes
        // and doubling all double-quote characters. See upstream comments for rationale.
        var reverse = new StringBuilder("\"");
        var quoteHit = true;
        for (var i = arg.Length; i > 0; i--)
        {
            var c = arg[i - 1];
            reverse.Append(c);
            if (quoteHit && c == '\\')
            {
                reverse.Append('\\');
            }
            else if (c == '"')
            {
                quoteHit = true;
                reverse.Append('"');
            }
            else
            {
                quoteHit = false;
            }
        }
        reverse.Append('"');

        // Reverse the StringBuilder.
        var chars = new char[reverse.Length];
        for (var i = 0; i < reverse.Length; i++)
        {
            chars[i] = reverse[reverse.Length - 1 - i];
        }
        return new string(chars);
    }

    /// <summary>
    /// Port of libuv's <c>quote_cmd_arg</c> (used by Node's <c>child_process.spawn</c>
    /// when launching a non-cmd executable on Windows). See upstream comments and
    /// licensing in <c>toolrunner.ts</c>.
    /// </summary>
    [SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "StringBuilder fluent calls return this.")]
    private static string UvQuoteCmdArg(string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            return "\"\"";
        }

        if (!arg.Contains(' ', StringComparison.Ordinal) &&
            !arg.Contains('\t', StringComparison.Ordinal) &&
            !arg.Contains('"', StringComparison.Ordinal))
        {
            return arg;
        }

        if (!arg.Contains('"', StringComparison.Ordinal) &&
            !arg.Contains('\\', StringComparison.Ordinal))
        {
            return $"\"{arg}\"";
        }

        var reverse = new StringBuilder("\"");
        var quoteHit = true;
        for (var i = arg.Length; i > 0; i--)
        {
            var c = arg[i - 1];
            reverse.Append(c);
            if (quoteHit && c == '\\')
            {
                reverse.Append('\\');
            }
            else if (c == '"')
            {
                quoteHit = true;
                reverse.Append('\\');
            }
            else
            {
                quoteHit = false;
            }
        }
        reverse.Append('"');

        var chars = new char[reverse.Length];
        for (var i = 0; i < reverse.Length; i++)
        {
            chars[i] = reverse[reverse.Length - 1 - i];
        }
        return new string(chars);
    }

    /// <summary>
    /// Executes the configured tool and returns the resulting exit code. Throws on
    /// failures unless <see cref="ExecOptions.IgnoreReturnCode"/> is set.
    /// </summary>
    public async ValueTask<int> ExecAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Resolve a relative tool path against options.cwd / process.cwd().
        if (!Path.IsPathRooted(_toolPath) &&
            (_toolPath.Contains('/', StringComparison.Ordinal) ||
             (s_isWindows && _toolPath.Contains('\\', StringComparison.Ordinal))))
        {
            var basePath = _options.Cwd ?? Environment.CurrentDirectory;
            if (!Path.IsPathRooted(basePath))
            {
                basePath = Path.GetFullPath(basePath, Environment.CurrentDirectory);
            }
            _toolPath = Path.GetFullPath(_toolPath, basePath);
        }

        // Resolve the tool via PATH / probe extensions on Windows.
        _toolPath = ResolveToolPath(_toolPath);

        // Validate cwd existence (mirrors upstream friendly error).
        if (!string.IsNullOrEmpty(_options.Cwd) && !Directory.Exists(_options.Cwd))
        {
            throw new InvalidOperationException($"The cwd: {_options.Cwd} does not exist!");
        }

        Debug($"exec tool: {_toolPath}");
        Debug("arguments:");
        foreach (var arg in _args)
        {
            Debug($"   {arg}");
        }

        var outStream = _options.OutStream ?? Console.Out;
        var errStream = _options.ErrStream ?? Console.Error;

        if (!_options.Silent)
        {
            outStream.WriteLine(GetCommandString(_options));
        }

        var fileName = GetSpawnFileName();
        var spawnArgs = GetSpawnArgs(_options);

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = _options.Input.HasValue,
            CreateNoWindow = true,
            // Critical: do NOT decode bytes as text — we read raw streams to drive listeners.
        };

        if (!string.IsNullOrEmpty(_options.Cwd))
        {
            psi.WorkingDirectory = _options.Cwd;
        }

        if (_options.Env is not null)
        {
            psi.EnvironmentVariables.Clear();
            foreach (var kvp in _options.Env)
            {
                psi.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
        }

        if (s_isWindows && _options.WindowsVerbatimArguments)
        {
            // .NET respects ProcessStartInfo.Arguments verbatim on Windows — no quoting.
            psi.Arguments = string.Join(' ', spawnArgs);
        }
        else if (s_isWindows && IsCmdFile())
        {
            // Cmd file requires the single pre-built /D /S /C "..." line, verbatim.
            // Use Arguments (raw) rather than ArgumentList so .NET does not re-quote.
            psi.Arguments = spawnArgs[0];
        }
        else
        {
            // Use ArgumentList so .NET applies platform-correct quoting from a token array.
            foreach (var a in spawnArgs)
            {
                psi.ArgumentList.Add(a);
            }
        }

        using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

        var stdoutBuffer = new StringBuilder();
        var stderrBuffer = new StringBuilder();
        var stderrWritten = false;
        Exception? readException = null;

        if (!proc.Start())
        {
            throw new InvalidOperationException(
                $"There was an error when attempting to execute the process '{_toolPath}'. This may indicate the process failed to start.");
        }

        var stdoutTask = Task.Run(async () =>
        {
            try
            {
                await PumpStreamAsync(
                    proc.StandardOutput.BaseStream,
                    stdoutBuffer,
                    isStderr: false,
                    outStream,
                    errStream,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                readException = ex;
            }
        }, CancellationToken.None);

        var stderrTask = Task.Run(async () =>
        {
            try
            {
                await PumpStreamAsync(
                    proc.StandardError.BaseStream,
                    stderrBuffer,
                    isStderr: true,
                    outStream,
                    errStream,
                    cancellationToken,
                    onByteWritten: () => stderrWritten = true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                readException = ex;
            }
        }, CancellationToken.None);

        // Write stdin.
        if (_options.Input.HasValue)
        {
            try
            {
                await proc.StandardInput.BaseStream.WriteAsync(_options.Input.Value, cancellationToken).ConfigureAwait(false);
                await proc.StandardInput.BaseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                proc.StandardInput.Close();
            }
        }

        // Cancellation: kill the process tree.
        await using var ctRegistration = cancellationToken.Register(() =>
        {
            try { proc.Kill(entireProcessTree: true); }
            catch { /* best effort */ }
        }).ConfigureAwait(false);

        await proc.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        // Wait for stdio drain with the configured delay (mirrors upstream STDIO-grace timeout).
        var delay = Math.Max(0, _options.Delay);
        var drainTask = Task.WhenAll(stdoutTask, stderrTask);
        var completed = await Task.WhenAny(drainTask, Task.Delay(delay, CancellationToken.None)).ConfigureAwait(false);
        if (completed != drainTask)
        {
            Debug($"The STDIO streams did not close within {delay / 1000} seconds of the exit event from process '{_toolPath}'. This may indicate a child process inherited the STDIO streams and has not yet exited.");
        }

        if (readException is not null)
        {
            throw readException;
        }

        // Flush any remaining buffered line into stdline/errline listeners.
        // (Pump already does this on EOF, but called here for parity with upstream's "emit on done" behavior.)

        Debug($"Exit code {proc.ExitCode} received from tool '{_toolPath}'");
        Debug($"STDIO streams have closed for tool '{_toolPath}'");

        if (proc.ExitCode != 0 && !_options.IgnoreReturnCode)
        {
            throw new InvalidOperationException(
                $"The process '{_toolPath}' failed with exit code {proc.ExitCode}");
        }

        if (stderrWritten && _options.FailOnStdErr)
        {
            throw new InvalidOperationException(
                $"The process '{_toolPath}' failed because one or more lines were written to the STDERR stream");
        }

        return proc.ExitCode;
    }

    private async Task PumpStreamAsync(
        Stream source,
        StringBuilder _,
        bool isStderr,
        TextWriter outStream,
        TextWriter errStream,
        CancellationToken cancellationToken,
        Action? onByteWritten = null)
    {
        var buffer = new byte[4096];
        var lineBuffer = string.Empty;

        while (true)
        {
            int read;
            try
            {
                read = await source.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException)
            {
                break;
            }

            if (read <= 0)
            {
                break;
            }

            onByteWritten?.Invoke();

            var slice = new ReadOnlyMemory<byte>(buffer, 0, read);
            var chunkText = Encoding.UTF8.GetString(buffer, 0, read);

            // Listener for raw bytes.
            if (isStderr)
            {
                _options.Listeners?.Stderr?.Invoke(slice);
            }
            else
            {
                _options.Listeners?.Stdout?.Invoke(slice);
            }

            // Live mirror to the configured streams (upstream silent-aware).
            if (!_options.Silent)
            {
                if (isStderr)
                {
                    var sink = _options.FailOnStdErr ? errStream : outStream;
                    sink.Write(chunkText);
                    sink.Flush();
                }
                else
                {
                    outStream.Write(chunkText);
                    outStream.Flush();
                }
            }

            // Line-buffered listener dispatch.
            lineBuffer = ProcessLineBuffer(chunkText, lineBuffer, line =>
            {
                if (isStderr)
                {
                    _options.Listeners?.Errline?.Invoke(line);
                }
                else
                {
                    _options.Listeners?.Stdline?.Invoke(line);
                }
            });
        }

        // Flush any trailing partial line (no newline) on EOF.
        if (lineBuffer.Length > 0)
        {
            if (isStderr)
            {
                _options.Listeners?.Errline?.Invoke(lineBuffer);
            }
            else
            {
                _options.Listeners?.Stdline?.Invoke(lineBuffer);
            }
        }
    }

    private string ProcessLineBuffer(string data, string strBuffer, Action<string> onLine)
    {
        try
        {
            var s = strBuffer + data;
            var eol = Environment.NewLine;
            var n = s.IndexOf(eol, StringComparison.Ordinal);

            // Also be tolerant of LF-only line endings on platforms where the child's stdio is LF.
            if (n < 0 && eol != "\n")
            {
                n = s.IndexOf('\n', StringComparison.Ordinal);
                while (n > -1)
                {
                    var line = s[..n];
                    onLine(line);
                    s = s[(n + 1)..];
                    n = s.IndexOf('\n', StringComparison.Ordinal);
                }
                return s;
            }

            while (n > -1)
            {
                var line = s[..n];
                onLine(line);
                s = s[(n + eol.Length)..];
                n = s.IndexOf(eol, StringComparison.Ordinal);
            }

            return s;
        }
        catch (Exception err)
        {
            // Streaming lines is best effort. Don't fail the build.
            Debug($"error processing line. Failed with error {err}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Resolve a tool name to an absolute path. If <paramref name="tool"/> is rooted,
    /// returns it (probing PATHEXT on Windows when no extension is present). Otherwise
    /// scans <c>PATH</c>.
    /// </summary>
    private static string ResolveToolPath(string tool)
    {
        if (Path.IsPathRooted(tool))
        {
            if (File.Exists(tool))
            {
                return tool;
            }

            if (s_isWindows)
            {
                var resolved = ProbePathExt(tool);
                if (resolved is not null)
                {
                    return resolved;
                }
            }

            throw new FileNotFoundException(
                $"Unable to locate executable file: {tool}. Please verify either the file path exists or the file can be found within a directory specified by the PATH environment variable. Also check the file mode to verify the file is executable.",
                tool);
        }

        // Relative paths with a directory component were already absolutized by caller.
        // Pure tool names are looked up via PATH.
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = Path.Combine(dir, tool);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                if (s_isWindows)
                {
                    var resolved = ProbePathExt(candidate);
                    if (resolved is not null)
                    {
                        return resolved;
                    }
                }
            }
        }

        throw new FileNotFoundException(
            $"Unable to locate executable file: {tool}. Please verify either the file path exists or the file can be found within a directory specified by the PATH environment variable. Also check the file mode to verify the file is executable.",
            tool);
    }

    private static string? ProbePathExt(string filePath)
    {
        var pathExt = Environment.GetEnvironmentVariable("PATHEXT");
        if (string.IsNullOrEmpty(pathExt))
        {
            return null;
        }

        // Probe filePath + ext for each ext in PATHEXT.
        foreach (var ext in pathExt.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = filePath + ext;
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
