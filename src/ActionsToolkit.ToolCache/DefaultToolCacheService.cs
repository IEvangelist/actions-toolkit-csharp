// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace ActionsToolkit.ToolCache;

/// <summary>
/// Default <see cref="IToolCacheService"/> implementation. Mirrors the
/// upstream <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/src/tool-cache.ts">
/// <c>@actions/tool-cache/tool-cache.ts</c></a> behavior; archive
/// extraction is delegated to <a href="https://github.com/adamhathcock/sharpcompress">
/// SharpCompress</a> instead of shelling out to <c>tar</c>/<c>7z</c>.
/// </summary>
internal sealed class DefaultToolCacheService(
    IHttpClient httpClient,
    IRetryHelper retryHelper) : IToolCacheService
{
    private const string UserAgent = "actions/tool-cache";

    public async ValueTask<string> DownloadToolAsync(
        string url,
        string? destination = null,
        string? auth = null,
        IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        var dest = destination ?? Path.Combine(ToolCacheLayout.GetTempDirectory(), Guid.NewGuid().ToString("N"));
        var parent = Path.GetDirectoryName(dest);
        if (!string.IsNullOrEmpty(parent))
        {
            Directory.CreateDirectory(parent);
        }

        return await retryHelper.ExecuteAsync(
            async ct => await DownloadAttemptAsync(url, dest, auth, headers, ct).ConfigureAwait(false),
            err =>
            {
                if (err is HttpError httpErr && httpErr.HttpStatusCode is { } status)
                {
                    var code = (int)status;
                    if (code < 500 && code != 408 && code != 429)
                    {
                        return false;
                    }
                }
                return true;
            },
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<string> DownloadAttemptAsync(
        string url,
        string dest,
        string? auth,
        IDictionary<string, string>? headers,
        CancellationToken ct)
    {
        if (File.Exists(dest))
        {
            throw new IOException($"Destination file path {dest} already exists");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd(UserAgent);

        if (headers is not null)
        {
            foreach (var (key, value) in headers)
            {
                request.Headers.TryAddWithoutValidation(key, value);
            }
        }

        if (!string.IsNullOrEmpty(auth))
        {
            request.Headers.TryAddWithoutValidation("Authorization", auth);
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpError(ex.StatusCode);
        }

        try
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpError(response.StatusCode);
            }

            await using var src = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            var succeeded = false;
            try
            {
                await using (var fs = File.Create(dest))
                {
                    await src.CopyToAsync(fs, ct).ConfigureAwait(false);
                }
                succeeded = true;
                return dest;
            }
            finally
            {
                if (!succeeded && File.Exists(dest))
                {
                    try { File.Delete(dest); } catch { /* best effort */ }
                }
            }
        }
        finally
        {
            response.Dispose();
        }
    }

    public async ValueTask<string> ExtractTarAsync(
        string file,
        string? dest = null,
        string[]? flags = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        var d = await CreateExtractFolderAsync(dest).ConfigureAwait(false);

        // Auto-detect compression via SharpCompress; flags are accepted for
        // upstream parity but not interpreted (no native tar binary involved).
        _ = flags;
        await Task.Run(() =>
        {
            using var stream = File.OpenRead(file);
            using var reader = ReaderFactory.Open(stream);
            var opts = new ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = true,
                PreserveFileTime = true,
            };
            while (reader.MoveToNextEntry())
            {
                if (reader.Entry.IsDirectory)
                {
                    continue;
                }
                reader.WriteEntryToDirectory(d, opts);
            }
        }, cancellationToken).ConfigureAwait(false);

        return d;
    }

    public async ValueTask<string> ExtractZipAsync(
        string file,
        string? dest = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        var d = await CreateExtractFolderAsync(dest).ConfigureAwait(false);

        await Task.Run(() => ZipFile.ExtractToDirectory(file, d, overwriteFiles: true), cancellationToken)
            .ConfigureAwait(false);

        return d;
    }

    public async ValueTask<string> Extract7zAsync(
        string file,
        string? dest = null,
        string[]? _7zPath = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        _ = _7zPath; // accepted for upstream parity, not invoked.

        var d = await CreateExtractFolderAsync(dest).ConfigureAwait(false);

        await Task.Run(() =>
        {
            using var archive = ArchiveFactory.Open(file);
            var opts = new ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = true,
                PreserveFileTime = true,
            };
            foreach (var entry in archive.Entries)
            {
                if (entry.IsDirectory)
                {
                    continue;
                }
                entry.WriteToDirectory(d, opts);
            }
        }, cancellationToken).ConfigureAwait(false);

        return d;
    }

    public ValueTask<string> ExtractXarAsync(
        string file,
        string? dest = null,
        string[]? flags = null,
        CancellationToken cancellationToken = default)
    {
        _ = file; _ = dest; _ = flags; _ = cancellationToken;
        throw new PlatformNotSupportedException(
            "xar extraction is not supported by SharpCompress. " +
            "On macOS, shell out to the native 'xar' binary instead.");
    }

    public async ValueTask<string> CacheDirAsync(
        string sourceDir,
        string tool,
        string version,
        string? arch = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceDir);
        ArgumentException.ThrowIfNullOrEmpty(tool);
        ArgumentException.ThrowIfNullOrEmpty(version);

        if (!Directory.Exists(sourceDir))
        {
            throw new IOException("sourceDir is not a directory");
        }

        version = NpmVersion.Clean(version) ?? version;
        arch ??= ToolCacheLayout.GetDefaultArch();

        var destPath = CreateToolPath(tool, version, arch);

        await Task.Run(() =>
        {
            foreach (var item in Directory.EnumerateFileSystemEntries(sourceDir, "*", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(item);
                var target = Path.Combine(destPath, name);
                if (Directory.Exists(item))
                {
                    CopyDirectory(item, target);
                }
                else
                {
                    File.Copy(item, target, overwrite: true);
                }
            }
        }, cancellationToken).ConfigureAwait(false);

        CompleteToolPath(tool, version, arch);
        return destPath;
    }

    public async ValueTask<string> CacheFileAsync(
        string sourceFile,
        string targetFile,
        string tool,
        string version,
        string? arch = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFile);
        ArgumentException.ThrowIfNullOrEmpty(targetFile);
        ArgumentException.ThrowIfNullOrEmpty(tool);
        ArgumentException.ThrowIfNullOrEmpty(version);

        if (!File.Exists(sourceFile))
        {
            throw new IOException("sourceFile is not a file");
        }

        version = NpmVersion.Clean(version) ?? version;
        arch ??= ToolCacheLayout.GetDefaultArch();

        var destFolder = CreateToolPath(tool, version, arch);
        var destPath = Path.Combine(destFolder, targetFile);

        await Task.Run(() => File.Copy(sourceFile, destPath, overwrite: true), cancellationToken)
            .ConfigureAwait(false);

        CompleteToolPath(tool, version, arch);
        return destFolder;
    }

    public string Find(string toolName, string versionSpec, string? arch = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(toolName);
        ArgumentException.ThrowIfNullOrEmpty(versionSpec);

        arch ??= ToolCacheLayout.GetDefaultArch();

        if (!NpmVersion.IsExplicit(versionSpec))
        {
            var localVersions = FindAllVersions(toolName, arch);
            var match = EvaluateVersions(localVersions, versionSpec);
            versionSpec = match ?? string.Empty;
        }

        if (string.IsNullOrEmpty(versionSpec))
        {
            return string.Empty;
        }

        versionSpec = NpmVersion.Clean(versionSpec) ?? string.Empty;
        if (string.IsNullOrEmpty(versionSpec))
        {
            return string.Empty;
        }

        var cachePath = Path.Combine(ToolCacheLayout.GetCacheDirectory(), toolName, versionSpec, arch);
        if (Directory.Exists(cachePath) && File.Exists(ToolCacheLayout.GetMarkerPath(cachePath)))
        {
            return cachePath;
        }
        return string.Empty;
    }

    public IReadOnlyList<string> FindAllVersions(string toolName, string? arch = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(toolName);
        arch ??= ToolCacheLayout.GetDefaultArch();

        var versions = new List<string>();
        var toolPath = Path.Combine(ToolCacheLayout.GetCacheDirectory(), toolName);
        if (!Directory.Exists(toolPath))
        {
            return versions;
        }

        foreach (var child in Directory.EnumerateDirectories(toolPath))
        {
            var name = Path.GetFileName(child);
            if (!NpmVersion.IsExplicit(name)) continue;

            var fullPath = Path.Combine(toolPath, name, arch);
            if (Directory.Exists(fullPath) && File.Exists(ToolCacheLayout.GetMarkerPath(fullPath)))
            {
                versions.Add(name);
            }
        }

        return versions;
    }

    public async ValueTask<IReadOnlyList<IToolRelease>> GetManifestFromRepoAsync(
        string owner,
        string repo,
        string authToken,
        string branch = "master",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(owner);
        ArgumentException.ThrowIfNullOrEmpty(repo);

        var treeUrl = $"https://api.github.com/repos/{owner}/{repo}/git/trees/{branch}";
        var headers = new Dictionary<string, IEnumerable<string>>(StringComparer.Ordinal);
        if (!string.IsNullOrEmpty(authToken))
        {
            headers["Authorization"] = [authToken];
        }

        var typed = await httpClient.GetAsync(
            treeUrl,
            ManifestJsonContext.Default.GitHubTree,
            headers,
            cancellationToken).ConfigureAwait(false);

        var tree = typed.Result;
        if (tree is null)
        {
            return Array.Empty<IToolRelease>();
        }

        var manifestUrl = string.Empty;
        foreach (var item in tree.Tree)
        {
            if (item.Path == "versions-manifest.json")
            {
                manifestUrl = item.Url;
                break;
            }
        }

        if (string.IsNullOrEmpty(manifestUrl))
        {
            return Array.Empty<IToolRelease>();
        }

        // The blob URL returns base64-encoded content by default; the
        // application/vnd.github.VERSION.raw accept header asks for raw JSON.
        using var rawRequest = new HttpRequestMessage(HttpMethod.Get, manifestUrl);
        rawRequest.Headers.UserAgent.ParseAdd(UserAgent);
        rawRequest.Headers.TryAddWithoutValidation("Accept", "application/vnd.github.VERSION.raw");
        if (!string.IsNullOrEmpty(authToken))
        {
            rawRequest.Headers.TryAddWithoutValidation("Authorization", authToken);
        }

        var rawResponse = await httpClient.SendAsync(rawRequest, cancellationToken).ConfigureAwait(false);
        try
        {
            if (rawResponse.StatusCode != HttpStatusCode.OK)
            {
                return Array.Empty<IToolRelease>();
            }

            var raw = await rawResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            // Strip BOM, mirroring upstream.
            if (raw.Length > 0 && raw[0] == '\uFEFF')
            {
                raw = raw[1..];
            }

            try
            {
                var releases = JsonSerializer.Deserialize(raw, ManifestJsonContext.Default.ListToolRelease);
                if (releases is null)
                {
                    return Array.Empty<IToolRelease>();
                }
                return releases.Cast<IToolRelease>().ToList();
            }
            catch (JsonException)
            {
                return Array.Empty<IToolRelease>();
            }
        }
        finally
        {
            rawResponse.Dispose();
        }
    }

    public ValueTask<IToolRelease?> FindFromManifestAsync(
        string versionSpec,
        bool stable,
        IReadOnlyList<IToolRelease> manifest,
        string? archFilter = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(versionSpec);
        ArgumentNullException.ThrowIfNull(manifest);

        archFilter ??= ToolCacheLayout.GetDefaultArch();
        var platFilter = ToolCacheLayout.GetDefaultPlatform();

        IToolRelease? match = null;
        IToolReleaseFile? matchedFile = null;

        foreach (var candidate in manifest)
        {
            if (!NpmVersionRange.Satisfies(candidate.Version, versionSpec))
            {
                continue;
            }
            if (stable && !candidate.Stable)
            {
                continue;
            }

            foreach (var f in candidate.Files)
            {
                if (f.Arch != archFilter || f.Platform != platFilter)
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(f.PlatformVersion))
                {
                    var osVer = OsVersionResolver.GetOsVersion();
                    if (osVer != f.PlatformVersion && !NpmVersionRange.Satisfies(osVer, f.PlatformVersion))
                    {
                        continue;
                    }
                }
                matchedFile = f;
                match = candidate;
                break;
            }

            if (matchedFile is not null)
            {
                break;
            }
        }

        if (match is null || matchedFile is null)
        {
            return ValueTask.FromResult<IToolRelease?>(null);
        }

        // Clone the candidate, narrowing files to the single match.
        var trimmed = new ToolRelease
        {
            Version = match.Version,
            Stable = match.Stable,
            ReleaseUrl = match.ReleaseUrl,
        };
        if (matchedFile is ToolReleaseFile concrete)
        {
            trimmed.Files.Add(concrete);
        }
        else
        {
            trimmed.Files.Add(new ToolReleaseFile
            {
                Filename = matchedFile.Filename,
                Platform = matchedFile.Platform,
                PlatformVersion = matchedFile.PlatformVersion,
                Arch = matchedFile.Arch,
                DownloadUrl = matchedFile.DownloadUrl,
            });
        }
        return ValueTask.FromResult<IToolRelease?>(trimmed);
    }

    public string? EvaluateVersions(IEnumerable<string> versions, string versionSpec)
    {
        ArgumentNullException.ThrowIfNull(versions);
        ArgumentException.ThrowIfNullOrEmpty(versionSpec);

        // Sort ascending by semver; iterate from highest to lowest.
        var parsed = versions
            .Select(s => (Raw: s, Parsed: NpmVersion.TryParse(s, out var v) ? v : (NpmVersion?)null))
            .Where(x => x.Parsed is not null)
            .OrderBy(x => x.Parsed!.Value)
            .ToList();

        for (var i = parsed.Count - 1; i >= 0; i--)
        {
            var candidate = parsed[i];
            if (NpmVersionRange.Satisfies(candidate.Raw, versionSpec))
            {
                return candidate.Raw;
            }
        }

        return null;
    }

    private static string CreateToolPath(string tool, string version, string arch)
    {
        var folder = Path.Combine(
            ToolCacheLayout.GetCacheDirectory(),
            tool,
            NpmVersion.Clean(version) ?? version,
            arch);
        var marker = ToolCacheLayout.GetMarkerPath(folder);
        if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        if (File.Exists(marker)) File.Delete(marker);
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static void CompleteToolPath(string tool, string version, string arch)
    {
        var folder = Path.Combine(
            ToolCacheLayout.GetCacheDirectory(),
            tool,
            NpmVersion.Clean(version) ?? version,
            arch);
        File.WriteAllText(ToolCacheLayout.GetMarkerPath(folder), string.Empty);
    }

    private static async ValueTask<string> CreateExtractFolderAsync(string? dest)
    {
        var d = string.IsNullOrEmpty(dest)
            ? Path.Combine(ToolCacheLayout.GetTempDirectory(), Guid.NewGuid().ToString("N"))
            : dest;
        Directory.CreateDirectory(d);
        await Task.CompletedTask.ConfigureAwait(false);
        return d;
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var dir in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dir.Replace(source, destination, StringComparison.Ordinal));
        }
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, file.Replace(source, destination, StringComparison.Ordinal), overwrite: true);
        }
    }
}
