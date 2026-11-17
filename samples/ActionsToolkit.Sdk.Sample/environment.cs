#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

if (args is ["produce"])
{
    var directory = Path.GetFullPath("path-tools");
    IO.MakeDirectory(directory);
    await Exec.RunAsync("dotnet", ["publish", "path-tool.cs", "-o", directory, "-p:PublishAot=false", "--nologo"]);
    await Core.ExportVariableAsync("CSHARP_SHOWCASE", "ready");
    await Core.AddPathAsync(directory);
    await Core.SetOutputAsync("expected", "ready");
}
else
{
    if (Environment.GetEnvironmentVariable("CSHARP_SHOWCASE") != Core.GetInput("expected", required: true))
        throw new InvalidOperationException("Exported environment was not visible to the next C# step.");
    var tool = IO.Which("path-tool");
    var result = await Exec.GetOutputAsync(tool);
    if (result.Stdout.Trim() != "CSharpPathReady")
        throw new InvalidOperationException("The PATH tool did not produce the expected result.");
    await Core.Summary.AddHeading("Environment and PATH").AddRaw($"Resolved `{tool}` in a later C# step.", true).WriteAsync();
}
