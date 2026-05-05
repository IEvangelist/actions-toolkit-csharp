// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.ToolCache.Tests;

/// <summary>
/// Mirrors the upstream
/// <a href="https://github.com/actions/toolkit/blob/main/packages/tool-cache/__tests__/manifest.test.ts">
/// <c>manifest.test.ts</c></a> suite, exercising
/// <see cref="IToolCacheService.FindFromManifestAsync"/>.
/// </summary>
public sealed class ManifestTests
{
    private static IToolCacheService NewService() =>
        new DefaultToolCacheService(
            httpClient: new StubHttpClient(new System.Net.Http.HttpClient()),
            retryHelper: new DefaultRetryHelper(maxAttempts: 1, minSeconds: 0, maxSeconds: 0,
                sleep: _ => Task.CompletedTask, info: _ => { }));

    private static List<IToolRelease> SampleManifest()
    {
        return
        [
            new ToolRelease
            {
                Version = "3.0.0",
                Stable = true,
                ReleaseUrl = "https://github.com/example/3.0.0",
                Files =
                {
                    new ToolReleaseFile { Filename = "tool-3.0.0-linux-x64.tar.gz", Platform = "linux", Arch = "x64", DownloadUrl = "https://example.test/3-linux" },
                    new ToolReleaseFile { Filename = "tool-3.0.0-win32-x64.zip", Platform = "win32", Arch = "x64", DownloadUrl = "https://example.test/3-win" },
                    new ToolReleaseFile { Filename = "tool-3.0.0-darwin-x64.tar.gz", Platform = "darwin", Arch = "x64", DownloadUrl = "https://example.test/3-mac" },
                },
            },
            new ToolRelease
            {
                Version = "2.0.0",
                Stable = true,
                ReleaseUrl = "https://github.com/example/2.0.0",
                Files =
                {
                    new ToolReleaseFile { Filename = "tool-2.0.0-linux-x64.tar.gz", Platform = "linux", Arch = "x64", DownloadUrl = "https://example.test/2-linux" },
                    new ToolReleaseFile { Filename = "tool-2.0.0-win32-x64.zip", Platform = "win32", Arch = "x64", DownloadUrl = "https://example.test/2-win" },
                    new ToolReleaseFile { Filename = "tool-2.0.0-darwin-x64.tar.gz", Platform = "darwin", Arch = "x64", DownloadUrl = "https://example.test/2-mac" },
                },
            },
            new ToolRelease
            {
                Version = "1.5.0-rc.1",
                Stable = false,
                ReleaseUrl = "https://github.com/example/1.5.0-rc.1",
                Files =
                {
                    new ToolReleaseFile { Filename = "tool-1.5.0-linux-x64.tar.gz", Platform = "linux", Arch = "x64", DownloadUrl = "https://example.test/1.5-linux" },
                },
            },
        ];
    }

    private static string CurrentPlatform() => ToolCacheLayout.GetDefaultPlatform();
    private static string CurrentArch() => "x64";

    [Fact(DisplayName = "findFromManifest matches highest version satisfying spec")]
    public async Task FindFromManifestMatchesHighest()
    {
        var svc = NewService();
        var manifest = SampleManifest();
        var match = await svc.FindFromManifestAsync("2.x || 3.x", stable: true, manifest, archFilter: CurrentArch());
        Assert.NotNull(match);
        Assert.Equal("3.0.0", match!.Version);
    }

    [Fact(DisplayName = "findFromManifest exact version")]
    public async Task FindFromManifestExactVersion()
    {
        var svc = NewService();
        var manifest = SampleManifest();
        var match = await svc.FindFromManifestAsync("2.0.0", stable: true, manifest, archFilter: CurrentArch());
        Assert.NotNull(match);
        Assert.Equal("2.0.0", match!.Version);
    }

    [Fact(DisplayName = "findFromManifest skips unstable when stable required")]
    public async Task FindFromManifestSkipsUnstable()
    {
        var svc = NewService();
        var manifest = SampleManifest();
        var match = await svc.FindFromManifestAsync("1.x", stable: true, manifest, archFilter: CurrentArch());
        Assert.Null(match);
    }

    [Fact(DisplayName = "findFromManifest returns null when no version satisfies")]
    public async Task FindFromManifestReturnsNullWhenNoMatch()
    {
        var svc = NewService();
        var manifest = SampleManifest();
        var match = await svc.FindFromManifestAsync("9.x", stable: true, manifest, archFilter: CurrentArch());
        Assert.Null(match);
    }

    [Fact(DisplayName = "findFromManifest skips files for other archs")]
    public async Task FindFromManifestFiltersArch()
    {
        var svc = NewService();
        var manifest = SampleManifest();
        var match = await svc.FindFromManifestAsync("3.0.0", stable: true, manifest, archFilter: "ppc64");
        Assert.Null(match);
    }

    [Fact(DisplayName = "findFromManifest narrows files to single match")]
    public async Task FindFromManifestNarrowsFiles()
    {
        var svc = NewService();
        var manifest = SampleManifest();
        var match = await svc.FindFromManifestAsync("3.0.0", stable: true, manifest, archFilter: CurrentArch());
        Assert.NotNull(match);
        Assert.Single(match!.Files);
        Assert.Equal(CurrentPlatform(), match.Files[0].Platform);
        Assert.Equal(CurrentArch(), match.Files[0].Arch);
    }

    [Fact(DisplayName = "findFromManifest skips releases lacking matching file")]
    public async Task FindFromManifestSkipsReleasesWithoutMatchingFile()
    {
        var svc = NewService();
        var manifest = new List<IToolRelease>
        {
            new ToolRelease
            {
                Version = "5.0.0",
                Stable = true,
                Files =
                {
                    new ToolReleaseFile { Filename = "tool-5.0.0-aix.tar.gz", Platform = "aix", Arch = "x64", DownloadUrl = "https://example.test/aix" },
                },
            },
            new ToolRelease
            {
                Version = "4.0.0",
                Stable = true,
                Files =
                {
                    new ToolReleaseFile { Filename = "tool-4.0.0-current.tar.gz", Platform = CurrentPlatform(), Arch = CurrentArch(), DownloadUrl = "https://example.test/4" },
                },
            },
        };

        var match = await svc.FindFromManifestAsync("*", stable: true, manifest, archFilter: CurrentArch());
        Assert.NotNull(match);
        Assert.Equal("4.0.0", match!.Version);
    }

    [Fact(DisplayName = "findFromManifest matches platform_version on linux")]
    public async Task FindFromManifestMatchesPlatformVersionOnLinux()
    {
        if (!OperatingSystem.IsLinux())
        {
            return; // skip — platform-specific
        }

        OsVersionResolver.LinuxVersionFileOverride = () =>
            "DISTRIB_RELEASE=18.04\nDISTRIB_ID=Ubuntu\n";

        try
        {
            var svc = NewService();
            var manifest = new List<IToolRelease>
            {
                new ToolRelease
                {
                    Version = "1.0.0",
                    Stable = true,
                    Files =
                    {
                        new ToolReleaseFile { Filename = "tool-22.tar.gz", Platform = "linux", PlatformVersion = "22.04", Arch = "x64", DownloadUrl = "u22" },
                        new ToolReleaseFile { Filename = "tool-18.tar.gz", Platform = "linux", PlatformVersion = "18.04", Arch = "x64", DownloadUrl = "u18" },
                    },
                },
            };

            var match = await svc.FindFromManifestAsync("1.0.0", stable: true, manifest, archFilter: "x64");
            Assert.NotNull(match);
            Assert.Equal("18.04", match!.Files[0].PlatformVersion);
        }
        finally
        {
            OsVersionResolver.LinuxVersionFileOverride = null;
        }
    }

    [Fact(DisplayName = "findFromManifest skips file when platform_version mismatches")]
    public async Task FindFromManifestSkipsFileWhenPlatformVersionMismatches()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        OsVersionResolver.LinuxVersionFileOverride = () =>
            "DISTRIB_RELEASE=20.04\nDISTRIB_ID=Ubuntu\n";

        try
        {
            var svc = NewService();
            var manifest = new List<IToolRelease>
            {
                new ToolRelease
                {
                    Version = "1.0.0",
                    Stable = true,
                    Files =
                    {
                        new ToolReleaseFile { Filename = "tool-22.tar.gz", Platform = "linux", PlatformVersion = "22.04", Arch = "x64", DownloadUrl = "u22" },
                    },
                },
            };

            var match = await svc.FindFromManifestAsync("1.0.0", stable: true, manifest, archFilter: "x64");
            Assert.Null(match);
        }
        finally
        {
            OsVersionResolver.LinuxVersionFileOverride = null;
        }
    }

    [Fact(DisplayName = "findFromManifest throws ArgumentException for empty spec")]
    public async Task FindFromManifestThrowsForEmptySpec()
    {
        var svc = NewService();
        var manifest = SampleManifest();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.FindFromManifestAsync(string.Empty, stable: true, manifest, archFilter: CurrentArch()).AsTask());
    }

    [Fact(DisplayName = "findFromManifest throws ArgumentNullException for null manifest")]
    public async Task FindFromManifestThrowsForNullManifest()
    {
        var svc = NewService();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            svc.FindFromManifestAsync("1.x", stable: true, manifest: null!, archFilter: CurrentArch()).AsTask());
    }

    [Fact(DisplayName = "OsVersionResolver parses VERSION_ID from os-release")]
    public void OsVersionResolverParsesVersionId()
    {
        OsVersionResolver.LinuxVersionFileOverride = () =>
            "NAME=\"Ubuntu\"\nVERSION_ID=\"22.04\"\nDISTRIB_RELEASE=20.04\n";
        try
        {
            if (OperatingSystem.IsLinux())
            {
                Assert.Equal("22.04", OsVersionResolver.GetOsVersion());
            }
        }
        finally
        {
            OsVersionResolver.LinuxVersionFileOverride = null;
        }
    }
}
