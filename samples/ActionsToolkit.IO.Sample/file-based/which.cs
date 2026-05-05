// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// which.cs
//
// Mirrors the upstream `@actions/io` README "which" snippet:
// https://github.com/actions/toolkit/tree/main/packages/io#which
//
// Resolves a tool's path via PATH. Returns the first match, mirroring the
// behaviour of `which`. Use `FileInPath(tool)` if you need every match.
//
// Run with:
//   dotnet run which.cs

#:package ActionsToolkit.IO@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.IO;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsIO()
    .BuildServiceProvider();

var io = provider.GetRequiredService<IOperations>();

// On Windows the `dotnet` CLI is virtually always on PATH; on Linux/macOS
// runners the same is true.
var dotnetPath = io.Which("dotnet");

if (string.IsNullOrEmpty(dotnetPath))
{
    Console.Error.WriteLine("Could not locate 'dotnet' on PATH.");
    Environment.Exit(1);
}

Console.WriteLine($"dotnet -> {dotnetPath}");

// FileInPath returns every match, not just the first.
var allMatches = io.FileInPath("dotnet");
Console.WriteLine($"All matches ({allMatches.Length}):");
foreach (var match in allMatches)
{
    Console.WriteLine($"  {match}");
}
