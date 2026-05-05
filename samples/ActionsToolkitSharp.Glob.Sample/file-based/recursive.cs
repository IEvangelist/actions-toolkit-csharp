// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// recursive.cs
//
// Mirrors the upstream `@actions/glob` README "Get all files recursively"
// snippet (the equivalent of `glob.create('**')`):
// https://github.com/actions/toolkit/tree/main/packages/glob
//
// Resolves every file under the current working directory recursively.
//
// Run with:
//   dotnet run recursive.cs

#:package ActionsToolkitSharp.Glob@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Glob;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsGlob()
    .BuildServiceProvider();

var globber = Globber.Create("**/*");
var files = globber.GlobFiles().ToList();

Console.WriteLine($"Found {files.Count} file(s) recursively in {Environment.CurrentDirectory}.");
foreach (var file in files.Take(10))
{
    Console.WriteLine($"  {file}");
}

if (files.Count > 10)
{
    Console.WriteLine($"  ... and {files.Count - 10} more");
}
