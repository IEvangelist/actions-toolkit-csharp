// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Models;

/// <summary>
/// Options that control cache restoration. Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/src/options.ts"><c>DownloadOptions</c></see>
/// from <c>@actions/cache</c>.
/// </summary>
public sealed class RestoreCacheOptions
{
    /// <summary>
    /// When <c>true</c>, the restore call only checks if a matching cache
    /// entry exists and returns the matched key without actually downloading
    /// or extracting the archive. Defaults to <c>false</c>.
    /// </summary>
    public bool LookupOnly { get; init; }

    /// <summary>
    /// Number of parallel downloads used when streaming the archive from the
    /// signed download URL. Mirrors upstream <c>downloadConcurrency</c>.
    /// Defaults to <c>8</c>.
    /// </summary>
    public int? DownloadConcurrency { get; init; }

    /// <summary>
    /// Maximum time in milliseconds for each download request. Mirrors
    /// upstream <c>timeoutInMs</c>. Defaults to <c>30 000</c>.
    /// </summary>
    public int? TimeoutInMs { get; init; }

    /// <summary>
    /// Time in milliseconds after which a stuck download segment is aborted.
    /// Mirrors upstream <c>segmentTimeoutInMs</c>. Defaults to <c>600 000</c>
    /// (overridable via <c>SEGMENT_DOWNLOAD_TIMEOUT_MINS</c>).
    /// </summary>
    public int? SegmentTimeoutInMs { get; init; }
}
