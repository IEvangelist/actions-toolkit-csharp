// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.ToolCache.Manifest;

/// <summary>
/// A single tool release entry in a versions manifest. Mirrors the upstream
/// <c>IToolRelease</c> interface exposed by
/// <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/src/manifest.ts">
/// <c>@actions/tool-cache/manifest.ts</c></a>.
/// </summary>
public interface IToolRelease
{
    /// <summary>
    /// The semantic version of this release.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Whether this release is considered stable (pre-release versions are
    /// flagged with <see langword="false"/>).
    /// </summary>
    bool Stable { get; }

    /// <summary>
    /// A URL to the release page (e.g. a GitHub release tag).
    /// </summary>
    string ReleaseUrl { get; }

    /// <summary>
    /// All platform/architecture-specific download files for this release.
    /// </summary>
    IReadOnlyList<IToolReleaseFile> Files { get; }
}
