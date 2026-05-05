// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache;

/// <summary>
/// The .NET equivalent of the
/// <a href="https://github.com/actions/toolkit/tree/main/packages/tool-cache">
/// <c>@actions/tool-cache</c></a> Node package: download, cache, and
/// look-up of tools on the runner. Native AOT-friendly: no reflection-based
/// JSON, no dynamic code generation.
/// </summary>
public interface IToolCacheService
{
    /// <summary>
    /// Downloads a tool from <paramref name="url"/> and writes it to
    /// <paramref name="destination"/> (defaults to a random file under
    /// <c>RUNNER_TEMP</c>). Mirrors upstream <c>tc.downloadTool</c>.
    /// </summary>
    /// <param name="url">URL of the tool to download.</param>
    /// <param name="destination">Optional destination file path.</param>
    /// <param name="auth">Optional <c>Authorization</c> header value.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The path of the downloaded file.</returns>
    ValueTask<string> DownloadToolAsync(
        string url,
        string? destination = null,
        string? auth = null,
        IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts a (possibly-compressed) tar archive at <paramref name="file"/>
    /// into <paramref name="dest"/> (defaults to a random temp directory).
    /// Mirrors upstream <c>tc.extractTar</c>.
    /// </summary>
    /// <param name="file">The path to the tar archive.</param>
    /// <param name="dest">Optional destination directory.</param>
    /// <param name="flags">Optional informational tar flags. The .NET port
    /// auto-detects gzip/bzip2/xz/zstd via SharpCompress; flags are accepted
    /// for upstream parity but are not fed to a native <c>tar</c> binary.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The path of the destination directory.</returns>
    ValueTask<string> ExtractTarAsync(
        string file,
        string? dest = null,
        string[]? flags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts a zip archive at <paramref name="file"/> into
    /// <paramref name="dest"/>. Mirrors upstream <c>tc.extractZip</c>.
    /// </summary>
    /// <param name="file">The path to the zip archive.</param>
    /// <param name="dest">Optional destination directory.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The path of the destination directory.</returns>
    ValueTask<string> ExtractZipAsync(
        string file,
        string? dest = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts a 7z archive at <paramref name="file"/> into
    /// <paramref name="dest"/>. Mirrors upstream <c>tc.extract7z</c>.
    /// </summary>
    /// <param name="file">The path to the 7z archive.</param>
    /// <param name="dest">Optional destination directory.</param>
    /// <param name="_7zPath">Optional informational path to a 7z binary.
    /// The .NET port uses SharpCompress, so this argument is accepted for
    /// upstream parity but is not invoked.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The path of the destination directory.</returns>
    [SuppressMessage(
        "Naming",
        "CA1707:Identifiers should not contain underscores",
        Justification = "Mirrors upstream tc.extract7z's _7zPath parameter name verbatim for cross-language traceability.")]
    ValueTask<string> Extract7zAsync(
        string file,
        string? dest = null,
        string[]? _7zPath = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts a xar archive at <paramref name="file"/> into
    /// <paramref name="dest"/>. Throws
    /// <see cref="PlatformNotSupportedException"/> on the current
    /// SharpCompress build, which does not ship xar support — consumers
    /// should shell out to <c>xar</c> themselves on macOS.
    /// </summary>
    /// <param name="file">The path to the xar archive.</param>
    /// <param name="dest">Optional destination directory.</param>
    /// <param name="flags">Optional informational xar flags.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The path of the destination directory.</returns>
    ValueTask<string> ExtractXarAsync(
        string file,
        string? dest = null,
        string[]? flags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches a directory and installs it into the tool-cache. Mirrors
    /// upstream <c>tc.cacheDir</c>.
    /// </summary>
    /// <param name="sourceDir">The directory whose contents to cache.</param>
    /// <param name="tool">The tool name.</param>
    /// <param name="version">The tool version (semver).</param>
    /// <param name="arch">Optional architecture (defaults to current).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The path of the cached tool directory.</returns>
    ValueTask<string> CacheDirAsync(
        string sourceDir,
        string tool,
        string version,
        string? arch = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches a single file and installs it into the tool-cache as
    /// <paramref name="targetFile"/>. Mirrors upstream <c>tc.cacheFile</c>.
    /// </summary>
    /// <param name="sourceFile">The file whose contents to cache.</param>
    /// <param name="targetFile">The destination file name.</param>
    /// <param name="tool">The tool name.</param>
    /// <param name="version">The tool version (semver).</param>
    /// <param name="arch">Optional architecture (defaults to current).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The path of the cached tool directory.</returns>
    ValueTask<string> CacheFileAsync(
        string sourceFile,
        string targetFile,
        string tool,
        string version,
        string? arch = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the path to a tool version in the local tool-cache, or returns
    /// an empty string when missing. Mirrors upstream <c>tc.find</c>.
    /// </summary>
    /// <param name="toolName">The tool name.</param>
    /// <param name="versionSpec">The version (or semver range) spec.</param>
    /// <param name="arch">Optional architecture (defaults to current).</param>
    /// <returns>The on-disk path, or <see cref="string.Empty"/>.</returns>
    string Find(string toolName, string versionSpec, string? arch = null);

    /// <summary>
    /// Returns all explicit versions of <paramref name="toolName"/> that are
    /// fully cached on disk. Mirrors upstream <c>tc.findAllVersions</c>.
    /// </summary>
    IReadOnlyList<string> FindAllVersions(string toolName, string? arch = null);

    /// <summary>
    /// Downloads the <c>versions-manifest.json</c> from a GitHub repo's
    /// <paramref name="branch"/>. Mirrors upstream <c>tc.getManifestFromRepo</c>.
    /// </summary>
    ValueTask<IReadOnlyList<IToolRelease>> GetManifestFromRepoAsync(
        string owner,
        string repo,
        string authToken,
        string branch = "master",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the highest tool release in <paramref name="manifest"/> that
    /// satisfies <paramref name="versionSpec"/>. Mirrors upstream
    /// <c>tc.findFromManifest</c>.
    /// </summary>
    ValueTask<IToolRelease?> FindFromManifestAsync(
        string versionSpec,
        bool stable,
        IReadOnlyList<IToolRelease> manifest,
        string? archFilter = null);

    /// <summary>
    /// Returns the highest version in <paramref name="versions"/> that
    /// satisfies <paramref name="versionSpec"/>, or <see langword="null"/>
    /// when none match. Mirrors upstream <c>tc.evaluateVersions</c>.
    /// </summary>
    string? EvaluateVersions(IEnumerable<string> versions, string versionSpec);
}
