using ActionsToolkit.Glob;

internal sealed record FileEntry(string Path, string Extension, long Bytes);
internal sealed record ExtensionTotal(string Extension, int Files, long Bytes);
internal sealed record FileStatsReport(string[] Patterns, FileEntry[] Entries, ExtensionTotal[] Extensions)
{
    public long TotalBytes => Entries.Sum(file => file.Bytes);
}

internal static class FileStatistics
{
    internal static FileStatsReport Scan(string root, string[] patterns)
    {
        root = Path.GetFullPath(root);
        var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var paths = Globber.Create(patterns,
            ["**/.git/**", "**/bin/**", "**/obj/**", "**/artifacts/**", "**/.showcase/**"])
            .GlobFiles(root).Select(Path.GetFullPath).Distinct(comparer);
        var entries = new List<FileEntry>();
        foreach (var path in paths)
        {
            var relative = Path.GetRelativePath(root, path);
            if (Path.IsPathRooted(relative) || relative == ".." ||
                relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                throw new InvalidOperationException("Glob returned a file outside the repository.");
            var file = new FileInfo(path);
            if (file.LinkTarget is not null || HasLinkedParent(file.Directory, root, comparer))
                continue;
            var extension = file.Extension.Length == 0 ? "(none)" : file.Extension.ToLowerInvariant();
            entries.Add(new(relative.Replace('\\', '/'), extension, file.Length));
        }
        var files = entries.OrderBy(file => file.Path, StringComparer.Ordinal).ToArray();
        var extensions = files.GroupBy(file => file.Extension, StringComparer.Ordinal)
            .Select(group => new ExtensionTotal(group.Key, group.Count(), group.Sum(file => file.Bytes)))
            .OrderByDescending(group => group.Bytes).ThenBy(group => group.Extension, StringComparer.Ordinal).ToArray();
        return new(patterns, files, extensions);
    }

    private static bool HasLinkedParent(DirectoryInfo? directory, string root, StringComparer comparer)
    {
        while (directory is not null && !comparer.Equals(directory.FullName, root))
        {
            if (directory.LinkTarget is not null)
                return true;
            directory = directory.Parent;
        }
        return false;
    }
}
