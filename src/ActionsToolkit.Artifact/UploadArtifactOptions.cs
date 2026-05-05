// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact;

/// <summary>
/// Options for <see cref="IArtifactClient.UploadArtifactAsync"/>. Mirrors
/// upstream <c>UploadArtifactOptions</c> from <c>@actions/artifact</c>.
/// </summary>
public sealed record UploadArtifactOptions
{
    /// <summary>
    /// Optional retention period in days, after which the artifact is eligible
    /// for deletion. Min <c>1</c>, max <c>90</c> (or whatever the repository's
    /// configured maximum is). When unset the backend default applies. A value
    /// of <c>0</c> is treated as "use the default".
    /// </summary>
    public int? RetentionDays { get; init; }

    /// <summary>
    /// Zlib-style compression level (<c>0</c>–<c>9</c>) applied when building
    /// the zip archive. Mapped onto the four .NET
    /// <see cref="System.IO.Compression.CompressionLevel"/> values:
    /// <list type="bullet">
    ///   <item><c>0</c> → <see cref="System.IO.Compression.CompressionLevel.NoCompression"/></item>
    ///   <item><c>1</c>–<c>3</c> → <see cref="System.IO.Compression.CompressionLevel.Fastest"/></item>
    ///   <item><c>4</c>–<c>6</c> (default) → <see cref="System.IO.Compression.CompressionLevel.Optimal"/></item>
    ///   <item><c>7</c>–<c>9</c> → <see cref="System.IO.Compression.CompressionLevel.SmallestSize"/></item>
    /// </list>
    /// .NET's <c>System.IO.Compression</c> does not expose the full Zlib
    /// 0–9 ladder, so we collapse onto the closest match per band.
    /// </summary>
    public int? CompressionLevel { get; init; }
}
