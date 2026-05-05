// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Models;

/// <summary>
/// Options that control cache uploads. Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/src/options.ts"><c>UploadOptions</c></see>
/// from <c>@actions/cache</c>.
/// </summary>
public sealed class SaveCacheOptions
{
    /// <summary>
    /// Number of parallel cache upload workers used when streaming the
    /// archive to the signed upload URL. Defaults to <c>4</c> upstream
    /// (overridden to <c>8</c> on the V2 path); capped at <c>32</c>.
    /// </summary>
    public int? UploadConcurrency { get; init; }

    /// <summary>
    /// Maximum chunk size in bytes for cache upload. Defaults to <c>32 MiB</c>
    /// upstream (overridden to <c>64 MiB</c> on the V2 path); capped at
    /// <c>128 MiB</c>.
    /// </summary>
    public long? UploadChunkSize { get; init; }

    /// <summary>
    /// Archive size in bytes. Mirrors upstream <c>archiveSizeBytes</c>;
    /// populated by the client when the archive is built and used purely for
    /// progress reporting.
    /// </summary>
    public long? ArchiveSizeBytes { get; init; }
}
