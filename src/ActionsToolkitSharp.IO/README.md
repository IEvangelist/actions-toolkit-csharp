# `ActionsToolkitSharp.IO` package

To install the [`ActionsToolkitSharp.IO`](https://www.nuget.org/packages/ActionsToolkitSharp.IO) NuGet package:

```xml
<PackageReference Include="ActionsToolkitSharp.IO" Version="[Version]" />
```

Or use the [`dotnet add package`](https://learn.microsoft.com/dotnet/core/tools/dotnet-add-package) .NET CLI command:

```bash
dotnet add package ActionsToolkitSharp.IO
```

## Get the `IOperations` instance

To use `IOperations` in your .NET project, register the services with an `IServiceCollection` instance by calling `AddGitHubActionsIO` and then your consuming code can require the `IOperations` via constructor dependency injection.

```csharp
using Microsoft.Extensions.DependencyInjection;
using ActionsToolkitSharp.IO;

using var provider = new ServiceCollection()
    .AddGitHubActionsIO()
    .BuildServiceProvider();

var io = provider.GetRequiredService<IOperations>();
```

## `ActionsToolkitSharp.IO`

This was modified, but borrowed from the [_io/README.md_](https://github.com/actions/toolkit/blob/main/packages/io/README.md).

> Core functions for cli filesystem scenarios

## Usage

### `mkdir -p`

Recursively make a directory. Follows rules specified in [man mkdir](https://linux.die.net/man/1/mkdir) with the `-p` option specified:

```csharp
io.MakeDirectory("path/to/make");
```

### `cp` / `mv`

Copy or move files or folders. Follows rules specified in [man cp](https://linux.die.net/man/1/cp) and [man mv](https://linux.die.net/man/1/mv):

```csharp
// Recursive must be true for directories
var options = new CopyOptions(Recursive: true, Force: false);

io.Copy("path/to/directory", "path/to/dest", options);
io.Move("path/to/file", "path/to/dest");
```

### `rm -rf`

Remove a file or folder recursively. Follows rules specified in [man rm](https://linux.die.net/man/1/rm) with the `-r` and `-f` rules specified.

```csharp
io.Remove("path/to/directory");
io.Remove("path/to/file");
```

### `which`

Get the path to a tool and resolves via `PATH`. Follows the rules specified in [man which](https://linux.die.net/man/1/which).

```csharp
var pythonPath = io.Which("python");
// Use io.FileInPath("python") to get every match instead of just the first.
```

## Attribution

This package is a .NET port of the official [`@actions/io`](https://github.com/actions/toolkit/tree/main/packages/io) Node.js package by GitHub, licensed under the [MIT License](https://github.com/actions/toolkit/blob/main/LICENSE.md).
