// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// mkdir.cs
//
// Mirrors the upstream `@actions/io` README "mkdir -p" snippet:
// https://github.com/actions/toolkit/tree/main/packages/io#mkdir--p
//
// Recursively creates a directory. Equivalent to `mkdir -p path/to/make`.
//
// Run with:
//   dotnet run mkdir.cs

#:package ActionsToolkit.IO@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.IO;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsIO()
    .BuildServiceProvider();

var io = provider.GetRequiredService<IOperations>();

var target = Path.Combine(Path.GetTempPath(), "actions-toolkit-sharp-io-sample", "path", "to", "make");

io.MakeDirectory(target);

Console.WriteLine($"Created directory: {target}");
Console.WriteLine($"Exists: {Directory.Exists(target)}");
