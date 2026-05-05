// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Manifest;

/// <summary>
/// A single platform/architecture-specific download in an
/// <see cref="IToolRelease"/>. Mirrors the upstream
/// <c>IToolReleaseFile</c> interface exposed by
/// <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/src/manifest.ts">
/// <c>@actions/tool-cache/manifest.ts</c></a>.
/// </summary>
public interface IToolReleaseFile
{
    /// <summary>
    /// The asset filename (e.g. <c>sometool-1.2.3-linux-x64.tar.gz</c>).
    /// </summary>
    string Filename { get; }

    /// <summary>
    /// The platform identifier as reported by Node's <c>os.platform()</c>:
    /// one of <c>aix</c>, <c>darwin</c>, <c>freebsd</c>, <c>linux</c>,
    /// <c>openbsd</c>, <c>sunos</c>, or <c>win32</c>.
    /// </summary>
    string Platform { get; }

    /// <summary>
    /// Optional semantic version (range) of the platform OS that this file
    /// targets — typically used to pin Ubuntu / macOS distribution versions.
    /// </summary>
    string? PlatformVersion { get; }

    /// <summary>
    /// The architecture identifier as reported by Node's <c>os.arch()</c>:
    /// one of <c>arm</c>, <c>arm64</c>, <c>ia32</c>, <c>mips</c>,
    /// <c>mipsel</c>, <c>ppc</c>, <c>ppc64</c>, <c>s390</c>, <c>s390x</c>,
    /// <c>x32</c>, or <c>x64</c>.
    /// </summary>
    string Arch { get; }

    /// <summary>
    /// The URL to download the asset from.
    /// </summary>
    string DownloadUrl { get; }
}
