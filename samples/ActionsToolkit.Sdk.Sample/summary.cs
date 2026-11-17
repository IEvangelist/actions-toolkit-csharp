#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using System.Globalization;
using System.Net;

var root = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Directory.GetCurrentDirectory();
if (args is ["measure"])
{
    var count = Glob.Create("src/**/*.cs").GlobFiles(root).Count();
    await Core.SetOutputAsync("files", count.ToString(CultureInfo.InvariantCulture));
}
else
{
    var count = int.Parse(Core.GetInput("files", required: true), CultureInfo.InvariantCulture);
    if (count != Glob.Create("src/**/*.cs").GlobFiles(root).Count())
        throw new InvalidOperationException("Repository source count changed between steps.");
    var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY") ?? "local";
    await Core.Summary.AddHeading("Repository snapshot")
        .AddRaw("| Metric | Measured value |\n| --- | ---: |\n" +
            $"| C# source files | {count} |\n| Repository | {WebUtility.HtmlEncode(repository)} |", true)
        .WriteAsync();
}
