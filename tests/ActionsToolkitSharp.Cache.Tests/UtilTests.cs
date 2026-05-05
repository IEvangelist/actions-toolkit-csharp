// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Cache.Tests;

/// <summary>
/// Mirrors upstream
/// <see href="https://github.com/actions/toolkit/blob/main/packages/cache/__tests__/util.test.ts">
/// <c>__tests__/util.test.ts</c></see>. The TypeScript module exercises
/// <c>maskSigUrl()</c> and <c>maskSecretUrls()</c> from
/// <c>src/internal/shared/util.ts</c> — helpers that pluck the
/// <c>sig=</c> SAS query parameter out of signed Azure Blob URLs and
/// register both the raw and percent-encoded form with
/// <c>@actions/core.setSecret</c> so subsequent log lines are masked.
/// <para>
/// Documented parity gap — symbols not exposed in the C# port:
/// </para>
/// <list type="bullet">
///   <item><c>maskSigUrl(url)</c> — no equivalent. The C# library
///     consumes signed upload/download URLs directly without piping
///     them through a workflow-command secret-masker. The Actions
///     workflow runtime masks the URL when it is first emitted from
///     the Twirp response, so the in-process library doesn't need to
///     re-mask. (If the library ever logs the SAS URL, it would need
///     to call into <c>ActionsToolkitSharp.Core.SetSecret</c>.)</item>
///   <item><c>maskSecretUrls(body)</c> — no equivalent for the same
///     reason; the Twirp DTO is consumed in-process and never logged
///     by the library.</item>
/// </list>
/// To keep this file from "silently dropping coverage" we exercise the
/// nearby <see cref="CacheUtils"/> helpers that <em>are</em> ported from
/// upstream <c>cacheUtils.ts</c> but were not already covered by
/// <see cref="CacheUtilsTests"/>: <c>createTempDirectory()</c>,
/// <c>getArchiveFileSizeInBytes()</c>, <c>unlinkFile()</c>.
/// </summary>
[Collection(EnvironmentSensitiveCollection.Name)]
public sealed class UtilTests
{
    [Fact(DisplayName = "createTempDirectory returns a fresh, existing directory")]
    public void CreateTempDirectory_Creates_FreshDirectory()
    {
        using var _ = new EnvironmentScope("RUNNER_TEMP", null);

        var first = CacheUtils.CreateTempDirectory();
        var second = CacheUtils.CreateTempDirectory();

        try
        {
            Assert.True(Directory.Exists(first));
            Assert.True(Directory.Exists(second));
            Assert.NotEqual(first, second);
        }
        finally
        {
            try { Directory.Delete(first, recursive: true); } catch { /* best effort */ }
            try { Directory.Delete(second, recursive: true); } catch { /* best effort */ }
        }
    }

    [Fact(DisplayName = "createTempDirectory honors RUNNER_TEMP when set")]
    public void CreateTempDirectory_Uses_RunnerTemp()
    {
        var runnerTemp = Path.Combine(Path.GetTempPath(), "runner-temp-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(runnerTemp);
        using var _ = new EnvironmentScope("RUNNER_TEMP", runnerTemp);

        var dir = CacheUtils.CreateTempDirectory();

        try
        {
            Assert.True(Directory.Exists(dir));
            Assert.Equal(
                Path.GetFullPath(runnerTemp).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(Path.GetDirectoryName(dir)!).TrimEnd(Path.DirectorySeparatorChar));
        }
        finally
        {
            try { Directory.Delete(runnerTemp, recursive: true); } catch { /* best effort */ }
        }
    }

    [Fact(DisplayName = "getArchiveFileSizeInBytes returns the on-disk byte length")]
    public void GetArchiveFileSizeInBytes_Returns_Length()
    {
        var path = Path.Combine(Path.GetTempPath(), "archive-" + Guid.NewGuid().ToString("N") + ".bin");
        var bytes = new byte[4096];
        Random.Shared.NextBytes(bytes);
        File.WriteAllBytes(path, bytes);

        try
        {
            Assert.Equal(4096L, CacheUtils.GetArchiveFileSizeInBytes(path));
        }
        finally
        {
            try { File.Delete(path); } catch { /* best effort */ }
        }
    }

    [Fact(DisplayName = "getArchiveFileSizeInBytes throws for null/empty path")]
    public void GetArchiveFileSizeInBytes_Throws_OnEmpty()
    {
        Assert.Throws<ArgumentException>(() => CacheUtils.GetArchiveFileSizeInBytes(""));
        Assert.Throws<ArgumentNullException>(() => CacheUtils.GetArchiveFileSizeInBytes(null!));
    }

    [Fact(DisplayName = "unlinkFile (TryDelete) removes an existing file")]
    public void TryDelete_Removes_ExistingFile()
    {
        var path = Path.Combine(Path.GetTempPath(), "delete-" + Guid.NewGuid().ToString("N") + ".tmp");
        File.WriteAllText(path, "x");
        Assert.True(File.Exists(path));

        CacheUtils.TryDelete(path);

        Assert.False(File.Exists(path));
    }

    [Fact(DisplayName = "unlinkFile (TryDelete) is a no-op for null/empty/missing paths")]
    public void TryDelete_NoOp_OnMissing()
    {
        // None of these should throw.
        CacheUtils.TryDelete(null);
        CacheUtils.TryDelete("");
        CacheUtils.TryDelete(Path.Combine(
            Path.GetTempPath(), "definitely-does-not-exist-" + Guid.NewGuid().ToString("N")));
    }
}
