namespace ActionsToolkit.Sdk.Tests;

public sealed class FileStatisticsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"toolkit-stats-{Guid.NewGuid():N}");

    public FileStatisticsTests() => Directory.CreateDirectory(_root);

    [Fact]
    public void ScanDeduplicatesOverlappingPatternsAndGroupsExactBytes()
    {
        Write("src/one.cs", "abc");
        Write("src/two.cs", "12345");
        Write("src/tool.csproj", "xy");
        Write("src/bin/generated.cs", "ignored");
        Write("src/obj/generated.cs", "ignored");
        Write("artifacts/generated.cs", "ignored");
        Write(".git/hidden.cs", "ignored");
        Write(".showcase/staged.cs", "ignored");

        var report = FileStatistics.Scan(_root, ["**/*.cs", "src/*", "**/*.csproj"]);

        Assert.Equal(3, report.Entries.Length);
        Assert.Equal(10, report.TotalBytes);
        Assert.Collection(report.Extensions,
            item => Assert.Equal(new ExtensionTotal(".cs", 2, 8), item),
            item => Assert.Equal(new ExtensionTotal(".csproj", 1, 2), item));
        Assert.All(report.Entries, entry => Assert.False(Path.IsPathRooted(entry.Path)));
    }

    [Fact]
    public void NoMatchesProducesAnEmptyReport()
    {
        Write("readme.txt", "hello");
        var report = FileStatistics.Scan(_root, ["**/*.cs"]);
        Assert.Empty(report.Entries);
        Assert.Empty(report.Extensions);
        Assert.Equal(0, report.TotalBytes);
    }

    [Fact]
    public void LinkedFilesAreNotRead()
    {
        if (OperatingSystem.IsWindows())
            return; // Windows symlink creation requires a privilege not assumed by this test.
        Write("source.cs", "hello");
        File.CreateSymbolicLink(Path.Combine(_root, "alias.cs"), Path.Combine(_root, "source.cs"));
        var report = FileStatistics.Scan(_root, ["**/*.cs"]);
        Assert.Single(report.Entries);
        Assert.Equal("source.cs", report.Entries[0].Path);
    }

    public void Dispose()
    {
        Directory.Delete(_root, recursive: true);
        GC.SuppressFinalize(this);
    }

    private void Write(string relative, string value)
    {
        var path = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, value);
    }
}
