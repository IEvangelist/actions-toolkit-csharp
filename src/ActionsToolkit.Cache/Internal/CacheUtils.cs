// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Internal;

/// <summary>
/// Cache service version. The V2 service is the default for hosted runners.
/// </summary>
internal enum CacheServiceVersion
{
    /// <summary>Legacy <c>_apis/artifactcache</c> endpoint.</summary>
    V1,

    /// <summary>Twirp-based <c>github.actions.results.api.v1.CacheService</c> endpoint.</summary>
    V2,
}

/// <summary>
/// Compression method written to the cache version. Mirrors upstream
/// <c>CompressionMethod</c> in <c>@actions/cache/src/internal/constants.ts</c>.
/// </summary>
internal enum CompressionMethod
{
    /// <summary>Tar piped through zstandard (this client's only mode).</summary>
    ZstdWithoutLong,

    /// <summary>Legacy gzip mode (upstream fallback when zstd is missing).</summary>
    Gzip,
}

/// <summary>
/// AOT-clean port of <c>@actions/cache/src/internal/cacheUtils.ts</c> and
/// <c>config.ts</c>. Surfaces the small set of process-environment lookups,
/// validation helpers, and version computation used by the cache client.
/// </summary>
internal static class CacheUtils
{
    /// <summary>
    /// Maximum allowed length for a cache key. Mirrors upstream <c>checkKey</c>.
    /// </summary>
    internal const int MaxKeyLength = 512;

    /// <summary>
    /// Maximum number of keys (primary plus restore keys) that can be passed
    /// to <c>RestoreCache</c>. Mirrors upstream <c>checkKey</c>.
    /// </summary>
    internal const int MaxRestoreKeys = 10;

    /// <summary>
    /// Salt mixed into the cache version hash so the upstream toolkit can
    /// invalidate every cache by bumping it. Mirrors upstream
    /// <c>versionSalt = '1.0'</c>.
    /// </summary>
    internal const string VersionSalt = "1.0";

    private const string RuntimeTokenEnvironmentVariable = "ACTIONS_RUNTIME_TOKEN";
    private const string ResultsUrlEnvironmentVariable = "ACTIONS_RESULTS_URL";
    private const string CacheUrlEnvironmentVariable = "ACTIONS_CACHE_URL";
    private const string CacheServiceV2EnvironmentVariable = "ACTIONS_CACHE_SERVICE_V2";
    private const string GitHubServerUrlVariable = "GITHUB_SERVER_URL";
    private const string DefaultGitHubServerUrl = "https://github.com";

    /// <summary>
    /// Returns true when the host pointed to by <c>GITHUB_SERVER_URL</c> is a
    /// GitHub Enterprise Server (GHES) instance — i.e. not <c>github.com</c>,
    /// not under the <c>.ghe.com</c> dogfooding domain, and not a
    /// <c>.localhost</c> sandbox. Mirrors upstream <c>isGhes()</c>.
    /// </summary>
    public static bool IsGhes()
    {
        var value = Environment.GetEnvironmentVariable(GitHubServerUrlVariable);
        if (string.IsNullOrWhiteSpace(value))
        {
            value = DefaultGitHubServerUrl;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var url))
        {
            return false;
        }

        var hostname = url.Host.TrimEnd().ToUpperInvariant();
        var isGitHubHost = string.Equals(hostname, "GITHUB.COM", StringComparison.Ordinal);
        var isGheHost = hostname.EndsWith(".GHE.COM", StringComparison.Ordinal);
        var isLocalHost = hostname.EndsWith(".LOCALHOST", StringComparison.Ordinal);

        return !isGitHubHost && !isGheHost && !isLocalHost;
    }

    /// <summary>
    /// Resolves the cache service version this process should target.
    /// V2 is enabled when <c>ACTIONS_CACHE_SERVICE_V2</c> is truthy and the
    /// host is not GHES; V1 is the legacy fallback. Mirrors upstream
    /// <c>getCacheServiceVersion()</c>.
    /// </summary>
    public static CacheServiceVersion GetCacheServiceVersion()
    {
        if (IsGhes())
        {
            return CacheServiceVersion.V1;
        }

        var v2 = Environment.GetEnvironmentVariable(CacheServiceV2EnvironmentVariable);
        return string.IsNullOrEmpty(v2) ? CacheServiceVersion.V1 : CacheServiceVersion.V2;
    }

    /// <summary>
    /// Returns the cache service base URL for the resolved version. V2 always
    /// reads <c>ACTIONS_RESULTS_URL</c>; V1 prefers <c>ACTIONS_CACHE_URL</c>
    /// and falls back to <c>ACTIONS_RESULTS_URL</c>. Mirrors upstream
    /// <c>getCacheServiceURL()</c>.
    /// </summary>
    public static string? GetCacheServiceUrl()
    {
        switch (GetCacheServiceVersion())
        {
            case CacheServiceVersion.V2:
                return Environment.GetEnvironmentVariable(ResultsUrlEnvironmentVariable);
            case CacheServiceVersion.V1:
            default:
                var cacheUrl = Environment.GetEnvironmentVariable(CacheUrlEnvironmentVariable);
                if (!string.IsNullOrEmpty(cacheUrl))
                {
                    return cacheUrl;
                }

                return Environment.GetEnvironmentVariable(ResultsUrlEnvironmentVariable);
        }
    }

    /// <summary>
    /// Returns the runtime token used as bearer credential for Twirp calls.
    /// Throws when the env var is unset. Mirrors upstream
    /// <c>getRuntimeToken()</c>.
    /// </summary>
    public static string GetRuntimeToken()
    {
        var token = Environment.GetEnvironmentVariable(RuntimeTokenEnvironmentVariable);
        if (string.IsNullOrEmpty(token))
        {
            throw new CacheServiceUnavailableException(
                "Unable to get the ACTIONS_RUNTIME_TOKEN env variable");
        }

        return token;
    }

    /// <summary>
    /// Returns true when the cache feature is available — i.e. the V2/V1
    /// service URL env var is set in the current process. Mirrors upstream
    /// <c>isFeatureAvailable()</c>.
    /// </summary>
    public static bool IsFeatureAvailable()
    {
        return GetCacheServiceVersion() switch
        {
            CacheServiceVersion.V2 => !string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable(ResultsUrlEnvironmentVariable)),
            _ => !string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable(CacheUrlEnvironmentVariable)),
        };
    }

    /// <summary>
    /// Validates a list of cache paths. Throws
    /// <see cref="CacheValidationException"/> when null or empty. Mirrors
    /// upstream <c>checkPaths()</c>.
    /// </summary>
    public static void CheckPaths(IReadOnlyList<string>? paths)
    {
        if (paths is null || paths.Count == 0)
        {
            throw new CacheValidationException(
                "Path Validation Error: At least one directory or file path is required");
        }
    }

    /// <summary>
    /// Validates a cache key — must be ≤ 512 chars, must not contain commas.
    /// Throws <see cref="CacheValidationException"/> on failure. Mirrors
    /// upstream <c>checkKey()</c>.
    /// </summary>
    public static void CheckKey(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length > MaxKeyLength)
        {
            throw new CacheValidationException(
                $"Key Validation Error: {key} cannot be larger than {MaxKeyLength} characters.");
        }

        if (key.Contains(',', StringComparison.Ordinal))
        {
            throw new CacheValidationException(
                $"Key Validation Error: {key} cannot contain commas.");
        }
    }

    /// <summary>
    /// Computes the cache version — sha256 of the joined path list, the
    /// optional compression method, an optional <c>windows-only</c> guard,
    /// and the version salt. Mirrors upstream <c>getCacheVersion()</c>.
    /// </summary>
    /// <param name="paths">The list of paths the cache covers.</param>
    /// <param name="compressionMethod">The archive compression method.</param>
    /// <param name="enableCrossOsArchive">When false on Windows, adds a
    /// <c>windows-only</c> marker so the resulting cache cannot be restored
    /// on Linux / macOS.</param>
    public static string GetCacheVersion(
        IReadOnlyList<string> paths,
        CompressionMethod? compressionMethod = null,
        bool enableCrossOsArchive = false)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var components = new List<string>(paths.Count + 3);
        components.AddRange(paths);

        if (compressionMethod.HasValue)
        {
            components.Add(GetCompressionLabel(compressionMethod.Value));
        }

        if (OperatingSystem.IsWindows() && !enableCrossOsArchive)
        {
            components.Add("windows-only");
        }

        components.Add(VersionSalt);

        var joined = string.Join('|', components);
        var bytes = Encoding.UTF8.GetBytes(joined);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// File extension for a cache archive of the given compression method.
    /// Mirrors upstream <c>getCacheFileName()</c>.
    /// </summary>
    public static string GetCacheFileName(CompressionMethod method) =>
        method == CompressionMethod.Gzip ? "cache.tgz" : "cache.tzst";

    private static string GetCompressionLabel(CompressionMethod method) =>
        method switch
        {
            CompressionMethod.Gzip => "gzip",
            CompressionMethod.ZstdWithoutLong => "zstd-without-long",
            _ => method.ToString(),
        };

    /// <summary>
    /// Returns a fresh temp directory under <c>RUNNER_TEMP</c> (when set)
    /// or the OS temp dir. Mirrors upstream <c>createTempDirectory()</c>.
    /// </summary>
    public static string CreateTempDirectory()
    {
        var runnerTemp = Environment.GetEnvironmentVariable("RUNNER_TEMP");
        var baseDir = string.IsNullOrEmpty(runnerTemp) ? Path.GetTempPath() : runnerTemp;
        var dest = Path.Combine(baseDir, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dest);
        return dest;
    }

    /// <summary>
    /// Returns the size in bytes of the file at <paramref name="filePath"/>.
    /// Mirrors upstream <c>getArchiveFileSizeInBytes()</c>.
    /// </summary>
    public static long GetArchiveFileSizeInBytes(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        return new FileInfo(filePath).Length;
    }

    /// <summary>
    /// Best-effort delete that swallows IO errors. Mirrors upstream
    /// <c>unlinkFile()</c>.
    /// </summary>
    public static void TryDelete(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // best effort
        }
    }
}
