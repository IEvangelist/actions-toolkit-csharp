// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// rm-rf.cs
//
// Mirrors the upstream `@actions/io` README "rm -rf" snippet:
// https://github.com/actions/toolkit/tree/main/packages/io#rm--rf
//
// Removes a file or directory recursively, equivalent to `rm -rf`.
// Works for both files and directories.
//
// Run with:
//   dotnet run rm-rf.cs

#:package ActionsToolkitSharp.IO@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.IO;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsIO()
    .BuildServiceProvider();

var io = provider.GetRequiredService<IOperations>();

var directory = Path.Combine(Path.GetTempPath(), $"io-rm-rf-{Guid.NewGuid():N}");
var nested = Path.Combine(directory, "nested");
var file = Path.Combine(directory, "leaf.txt");

io.MakeDirectory(nested);
File.WriteAllText(file, "soon to be deleted");

Console.WriteLine($"Before: directory={Directory.Exists(directory)}, file={File.Exists(file)}");

// Remove a single file first.
io.Remove(file);
Console.WriteLine($"After file remove: file={File.Exists(file)}");

// Then remove the entire directory recursively.
io.Remove(directory);
Console.WriteLine($"After directory remove: directory={Directory.Exists(directory)}");
