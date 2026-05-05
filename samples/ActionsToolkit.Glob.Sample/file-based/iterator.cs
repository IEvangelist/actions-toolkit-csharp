// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// iterator.cs
//
// Mirrors the upstream `@actions/glob` README "Iterator" section:
// https://github.com/actions/toolkit/tree/main/packages/glob#iterator
//
// When dealing with a large amount of results, iterate the sequence
// returned by GlobFiles() instead of materialising it all at once.
//
// Run with:
//   dotnet run iterator.cs

#:package ActionsToolkit.Glob@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.Glob;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsGlob()
    .BuildServiceProvider();

var globber = Globber.Create("**/*");

var count = 0;
foreach (var file in globber.GlobFiles())
{
    Console.WriteLine(file);
    count++;
}

Console.WriteLine($"Streamed {count} file(s).");
