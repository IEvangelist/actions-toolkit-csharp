#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using ActionsToolkit.Core;

Core.StartGroup("Validate repository source");
try
{
    var root = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Directory.GetCurrentDirectory();
    var files = Glob.Create("src/**/*.cs").GlobFiles(root).ToArray();
    if (files.Length == 0)
        throw new InvalidOperationException("Expected repository source files.");
    Core.Notice($"Inspected {files.Length} C# files.", new AnnotationProperties { File = "README.md", StartLine = 1 });
    Core.Warning("Demonstration warning: no build failure is implied.");
    await Core.SetOutputAsync("notices", "1");
    await Core.SetOutputAsync("warnings", "1");
}
finally
{
    Core.EndGroup();
}
