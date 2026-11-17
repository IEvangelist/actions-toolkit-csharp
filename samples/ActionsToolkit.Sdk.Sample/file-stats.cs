#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk
#:include file-stats.helpers.cs

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

if (args is ["scan"])
{
    var root = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Directory.GetCurrentDirectory();
    var report = FileStatistics.Scan(root, ["**/*.cs", "**/*.csproj", "**/*.props", "**/*.targets"]);
    await File.WriteAllTextAsync("file-stats.json", JsonSerializer.Serialize(report, StatsJson.Default.FileStatsReport));
    await Core.SetOutputAsync("report", ".showcase/file-stats.json");
    await Core.SetOutputAsync("count", report.Entries.Length.ToString(CultureInfo.InvariantCulture));
    await Core.SetOutputAsync("bytes", report.TotalBytes.ToString(CultureInfo.InvariantCulture));
}
else
{
    var root = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Directory.GetParent(Directory.GetCurrentDirectory())!.FullName;
    var reportPath = Path.GetFullPath(Core.GetInput("report", required: true), root);
    if (reportPath != Path.Combine(root, ".showcase", "file-stats.json"))
        throw new InvalidOperationException("Unexpected statistics report path.");
    var report = JsonSerializer.Deserialize(await File.ReadAllTextAsync(reportPath), StatsJson.Default.FileStatsReport)
        ?? throw new InvalidOperationException("Missing statistics report.");
    if (report.Entries.Length != int.Parse(Core.GetInput("count", required: true), CultureInfo.InvariantCulture) ||
        report.TotalBytes != long.Parse(Core.GetInput("bytes", required: true), CultureInfo.InvariantCulture) ||
        report.Extensions.Sum(type => type.Files) != report.Entries.Length ||
        report.Extensions.Sum(type => type.Bytes) != report.TotalBytes)
        throw new InvalidOperationException("Statistics changed during the C# step handoff.");

    var markdown = new StringBuilder()
        .AppendLine($"**{report.Entries.Length:N0} files; {report.TotalBytes:N0} bytes.**")
        .AppendLine()
        .AppendLine($"Patterns: {Escape(string.Join(", ", report.Patterns))}")
        .AppendLine()
        .AppendLine("Generated output, git metadata, and linked paths are excluded.")
        .AppendLine()
        .AppendLine("| Extension | Files | Bytes |")
        .AppendLine("| --- | ---: | ---: |");
    foreach (var type in report.Extensions)
        markdown.AppendLine($"| {Escape(type.Extension)} | {type.Files:N0} | {type.Bytes:N0} |");
    markdown.AppendLine().AppendLine("### Ten largest files").AppendLine()
        .AppendLine("| Repository-relative path | Bytes |").AppendLine("| --- | ---: |");
    foreach (var file in report.Entries.OrderByDescending(file => file.Bytes).ThenBy(file => file.Path, StringComparer.Ordinal).Take(10))
        markdown.AppendLine($"| {Escape(file.Path)} | {file.Bytes:N0} |");
    if (report.Entries.Length == 0)
        markdown.AppendLine("| No matching files | 0 |");
    await Core.Summary.AddHeading("Repository filesystem statistics").AddRaw(markdown.ToString(), true).WriteAsync();
}

static string Escape(string value) => WebUtility.HtmlEncode(value).Replace("|", "&#124;", StringComparison.Ordinal)
    .Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal)
    .Replace("`", "&#96;", StringComparison.Ordinal).Replace("[", "&#91;", StringComparison.Ordinal)
    .Replace("]", "&#93;", StringComparison.Ordinal).Replace("*", "&#42;", StringComparison.Ordinal)
    .Replace("_", "&#95;", StringComparison.Ordinal);

[JsonSerializable(typeof(FileStatsReport))]
internal partial class StatsJson : JsonSerializerContext;
