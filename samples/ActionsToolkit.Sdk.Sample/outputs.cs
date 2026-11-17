#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using System.Text.Json;
using System.Text.Json.Serialization;

if (args is ["produce"])
{
    await Core.SetOutputAsync("message", "C# step one\nC# step two");
    await Core.SetOutputAsync("report", new BuildReport("Release", 3), ReportJson.Default.BuildReport);
}
else
{
    var message = Core.GetInput("message", required: true);
    var report = JsonSerializer.Deserialize(Core.GetInput("report", required: true), ReportJson.Default.BuildReport);
    if (message != "C# step one\nC# step two" || report is not { Configuration: "Release", Projects: 3 })
        throw new InvalidOperationException("The C# output-to-input handoff changed the values.");
    await Core.Summary.AddMarkdownHeading("C# to C#").AddNewLine()
        .AddMarkdownCodeBlock(message).AddNewLine()
        .AddRawMarkdown($"Verified typed report: {report.Configuration}, {report.Projects} projects.", true).WriteAsync();
}

public sealed record BuildReport(string Configuration, int Projects);

[JsonSerializable(typeof(BuildReport))]
internal partial class ReportJson : JsonSerializerContext;
