// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/__tests__/options.test.ts">
/// <c>__tests__/options.test.ts</c></see>. The TypeScript implementation
/// exposes <c>getDownloadOptions()</c> / <c>getUploadOptions()</c>
/// factory functions that fold caller-supplied overrides on top of
/// hard-coded defaults and clamp them against
/// <c>CACHE_UPLOAD_CONCURRENCY</c> /
/// <c>CACHE_UPLOAD_CHUNK_SIZE</c> /
/// <c>SEGMENT_DOWNLOAD_TIMEOUT_MINS</c> environment variables.
/// <para>
/// The C# port keeps <see cref="SaveCacheOptions"/> and
/// <see cref="RestoreCacheOptions"/> as plain POCOs whose unset
/// properties remain <see langword="null"/>; only
/// <see cref="RestoreCacheOptions.LookupOnly"/> is consumed by
/// <c>DefaultCacheClient</c> today. Documented parity gap:
/// </para>
/// <list type="bullet">
///   <item><c>getUploadOptions()</c> / <c>getDownloadOptions()</c>
///     factories with hard-coded defaults and env-var caps —
///     not yet ported (the orchestrator hands the archive directly
///     to <see cref="System.Net.Http.HttpClient"/> with framework
///     defaults). Tracked for follow-up parity.</item>
///   <item>Azure-SDK-specific options (<c>useAzureSdk</c>,
///     <c>concurrentBlobDownloads</c>) — not exposed; the C# blob
///     transport uses <see cref="System.Net.Http.HttpClient"/> only.</item>
/// </list>
/// </summary>
public sealed class OptionsTests
{
    [Fact(DisplayName = "SaveCacheOptions defaults to all-null POCO")]
    public void SaveCacheOptions_Defaults_AllNull()
    {
        var options = new SaveCacheOptions();

        Assert.Null(options.UploadConcurrency);
        Assert.Null(options.UploadChunkSize);
        Assert.Null(options.ArchiveSizeBytes);
    }

    [Fact(DisplayName = "SaveCacheOptions preserves caller-supplied init values")]
    public void SaveCacheOptions_Preserves_InitValues()
    {
        var options = new SaveCacheOptions
        {
            UploadConcurrency = 16,
            UploadChunkSize = 64L * 1024 * 1024,
            ArchiveSizeBytes = 1234L,
        };

        Assert.Equal(16, options.UploadConcurrency);
        Assert.Equal(64L * 1024 * 1024, options.UploadChunkSize);
        Assert.Equal(1234L, options.ArchiveSizeBytes);
    }

    [Fact(DisplayName = "RestoreCacheOptions defaults: LookupOnly=false, all sizes null")]
    public void RestoreCacheOptions_Defaults()
    {
        var options = new RestoreCacheOptions();

        Assert.False(options.LookupOnly);
        Assert.Null(options.DownloadConcurrency);
        Assert.Null(options.TimeoutInMs);
        Assert.Null(options.SegmentTimeoutInMs);
    }

    [Fact(DisplayName = "RestoreCacheOptions preserves caller-supplied init values")]
    public void RestoreCacheOptions_Preserves_InitValues()
    {
        var options = new RestoreCacheOptions
        {
            LookupOnly = true,
            DownloadConcurrency = 14,
            TimeoutInMs = 20_000,
            SegmentTimeoutInMs = 3_600_000,
        };

        Assert.True(options.LookupOnly);
        Assert.Equal(14, options.DownloadConcurrency);
        Assert.Equal(20_000, options.TimeoutInMs);
        Assert.Equal(3_600_000, options.SegmentTimeoutInMs);
    }

    [Fact(DisplayName = "Documented parity gap: no getUploadOptions/getDownloadOptions factory + env caps")]
    public void Documented_OptionsFactory_ParityGap()
    {
        // Upstream `options.test.ts` covers default-fill factories that read
        // CACHE_UPLOAD_CONCURRENCY / CACHE_UPLOAD_CHUNK_SIZE /
        // SEGMENT_DOWNLOAD_TIMEOUT_MINS and clamp the result. The C# library
        // currently treats these option records as plain POCOs and the
        // orchestrator only consumes `LookupOnly`. This assertion guards the
        // intentional gap so a future contributor adding the factory layer
        // gets a deliberate test failure to update.
        Assert.Null(typeof(SaveCacheOptions)
            .GetMethod("GetEffective", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
        Assert.Null(typeof(RestoreCacheOptions)
            .GetMethod("GetEffective", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
    }
}
