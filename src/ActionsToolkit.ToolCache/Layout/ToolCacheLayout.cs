// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Layout;

/// <summary>
/// Helpers for the <c>RUNNER_TOOL_CACHE</c>-rooted
/// <c>&lt;tool&gt;/&lt;version&gt;/&lt;arch&gt;</c> directory layout used by
/// the GitHub Actions runner and by upstream
/// <c>@actions/tool-cache</c>.
/// </summary>
public static class ToolCacheLayout
{
    /// <summary>
    /// The environment variable that identifies the on-disk root of the
    /// tool cache.
    /// </summary>
    public const string CacheDirectoryEnvVar = "RUNNER_TOOL_CACHE";

    /// <summary>
    /// The environment variable that identifies the runner's temp dir.
    /// </summary>
    public const string TempDirectoryEnvVar = "RUNNER_TEMP";

    /// <summary>
    /// Resolves the tool cache root directory from <c>RUNNER_TOOL_CACHE</c>.
    /// Throws when unset.
    /// </summary>
    public static string GetCacheDirectory()
    {
        var v = Environment.GetEnvironmentVariable(CacheDirectoryEnvVar);
        if (string.IsNullOrEmpty(v))
        {
            throw new InvalidOperationException(
                $"Expected {CacheDirectoryEnvVar} to be defined");
        }
        return v;
    }

    /// <summary>
    /// Resolves the runner temp directory from <c>RUNNER_TEMP</c>.
    /// Throws when unset.
    /// </summary>
    public static string GetTempDirectory()
    {
        var v = Environment.GetEnvironmentVariable(TempDirectoryEnvVar);
        if (string.IsNullOrEmpty(v))
        {
            throw new InvalidOperationException(
                $"Expected {TempDirectoryEnvVar} to be defined");
        }
        return v;
    }

    /// <summary>
    /// Returns the default architecture string used for cache layout when
    /// callers do not supply one. Maps
    /// <see cref="RuntimeInformation.OSArchitecture"/> to the lowercase
    /// Node-style identifiers (<c>x64</c>, <c>arm64</c>, <c>x86</c>, etc.).
    /// </summary>
    public static string GetDefaultArch() =>
        RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "ia32",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(),
        };

    /// <summary>
    /// Returns the platform string used for manifest matching, mirroring
    /// the values produced by Node's <c>os.platform()</c>: <c>win32</c>,
    /// <c>darwin</c>, <c>linux</c>, <c>freebsd</c>, etc.
    /// </summary>
    public static string GetDefaultPlatform()
    {
        if (OperatingSystem.IsWindows()) return "win32";
        if (OperatingSystem.IsMacOS()) return "darwin";
        if (OperatingSystem.IsLinux()) return "linux";
        if (OperatingSystem.IsFreeBSD()) return "freebsd";
        return RuntimeInformation.OSDescription.ToLowerInvariant();
    }

    /// <summary>
    /// Returns the directory path for the given <paramref name="tool"/>,
    /// <paramref name="version"/>, and <paramref name="arch"/>.
    /// </summary>
    public static string GetToolPath(string tool, string version, string? arch = null) =>
        Path.Combine(
            GetCacheDirectory(),
            tool,
            NpmVersion.Clean(version) ?? version,
            arch ?? GetDefaultArch());

    /// <summary>
    /// Returns the marker file path for a populated tool path. The marker
    /// signals that a previous cache write completed successfully.
    /// </summary>
    public static string GetMarkerPath(string toolPath) => $"{toolPath}.complete";
}
