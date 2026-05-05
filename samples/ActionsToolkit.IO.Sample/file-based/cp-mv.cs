// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// cp-mv.cs
//
// Mirrors the upstream `@actions/io` README "cp/mv" snippet:
// https://github.com/actions/toolkit/tree/main/packages/io#cpmv
//
// Demonstrates copying a directory recursively and then moving an
// individual file. Recursive must be true for directories.
//
// Run with:
//   dotnet run cp-mv.cs

#:package ActionsToolkit.IO@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.IO;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsIO()
    .BuildServiceProvider();

var io = provider.GetRequiredService<IOperations>();

var root = Path.Combine(Path.GetTempPath(), $"io-cp-mv-{Guid.NewGuid():N}");
var source = Path.Combine(root, "path", "to", "directory");
var destination = Path.Combine(root, "path", "to", "dest");

io.MakeDirectory(source);
File.WriteAllText(Path.Combine(source, "hello.txt"), "Hello!");

// Recursive must be true for directories. CopySourceDirectory: false
// matches the upstream `io.cp(src, dest, { recursive: true })` behavior
// of placing source contents directly into dest.
var options = new CopyOptions(Recursive: true, Force: false, CopySourceDirectory: false);
io.Copy(source, destination, options);

Console.WriteLine($"Copied {source} -> {destination}");
Console.WriteLine($"  hello.txt copied? {File.Exists(Path.Combine(destination, "hello.txt"))}");

// Move a single file.
var movedFrom = Path.Combine(destination, "hello.txt");
var movedTo = Path.Combine(destination, "hello-moved.txt");
io.Move(movedFrom, movedTo);

Console.WriteLine($"Moved {movedFrom} -> {movedTo}");
Console.WriteLine($"  hello-moved.txt exists? {File.Exists(movedTo)}");

// Cleanup.
Directory.Delete(root, recursive: true);
