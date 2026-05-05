# `ActionsToolkitSharp.IO` package

To install the [`ActionsToolkitSharp.IO`](https://www.nuget.org/packages/ActionsToolkitSharp.IO) NuGet package:

```xml
<PackageReference Include="ActionsToolkitSharp.IO" Version="[Version]" />
```

Or use the [`dotnet add package`](https://learn.microsoft.com/dotnet/core/tools/dotnet-add-package) .NET CLI command:

```bash
dotnet add package ActionsToolkitSharp.IO
```

## `ActionsToolkitSharp.IO`

This was modified, but borrowed from the [_glob/README.md_](https://github.com/actions/toolkit/blob/main/packages/io/README.md).

> Core functions for cli filesystem scenarios

## Usage

#### mkdir -p

Recursively make a directory. Follows rules specified in [man mkdir](https://linux.die.net/man/1/mkdir) with the `-p` option specified:

```csharp
using Action.IO;

await io.mkdirP("path/to/make");
```

#### cp/mv

Copy or move files or folders. Follows rules specified in [man cp](https://linux.die.net/man/1/cp) and [man mv](https://linux.die.net/man/1/mv):

```csharp
const io = require("@actions/io");

// Recursive must be true for directories
const options = { recursive: true, force: false }

await io.cp("path/to/directory", "path/to/dest", options);
await io.mv("path/to/file", "path/to/dest");
```

#### rm -rf

Remove a file or folder recursively. Follows rules specified in [man rm](https://linux.die.net/man/1/rm) with the `-r` and `-f` rules specified.

```csharp
const io = require("@actions/io");

await io.rmRF("path/to/directory");
await io.rmRF("path/to/file");
```

#### which

Get the path to a tool and resolves via paths. Follows the rules specified in [man which](https://linux.die.net/man/1/which).

```csharp
using Action.IO;

const exec = require("@actions/exec");
const io = require("@actions/io");

const pythonPath: string = await io.which("python", true)

await exec.exec(`"${pythonPath}"`, ["main.py"]);
```