// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// basic.cs
//
// Mirrors the upstream `@actions/glob` README "Basic" section:
// https://github.com/actions/toolkit/tree/main/packages/glob#basic
//
// Demonstrates creating a Globber from one or more inclusion patterns
// and materialising the results to a list of files.
//
// Run with:
//   dotnet run basic.cs

#:package ActionsToolkitSharp.Glob@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Glob;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsGlob()
    .BuildServiceProvider();

string[] patterns = ["**/tar.gz", "**/tar.bz"];

var globber = Globber.Create(patterns);
var files = globber.GlobFiles().ToList();

Console.WriteLine($"Matched {files.Count} file(s) for patterns: {string.Join(", ", patterns)}");
foreach (var file in files)
{
    Console.WriteLine($"  {file}");
}
