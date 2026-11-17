# ActionsToolkit.Sdk

An MSBuild SDK for GitHub Actions written in C#. Requires the .NET 11 SDK for
the primary file-based and stdin experience.

```csharp
#:sdk ActionsToolkit.Sdk@<version>

Core.Info("Hello from C#!");
await Core.SetOutputAsync("message", "Build complete");
```

Replace `<version>` with the exact installed/published toolkit version, then run
`dotnet run --file build.cs`. For stdin, pipe source to `dotnet run -` and include
the exact version in its `#:sdk ActionsToolkit.Sdk@<version>` directive. This
SDK supplies all ten toolkit libraries and module aliases; it does not install
the .NET SDK.

Traditional projects can use `<Project Sdk="ActionsToolkit.Sdk/<version>">`.
Alternatively, pin `ActionsToolkit.Sdk` in `global.json`'s `msbuild-sdks` map.
Do not specify conflicting SDK versions in the same build.

The matching `ActionsToolkit.Scripting` version is supplied automatically.
An explicit package reference takes precedence; central package management must
allow version overrides for the implicit reference, or provide its own explicit
reference. `ActionsToolkitVersion` can override the default matching version.

Aliases are ordinary MSBuild `Using` items. Remove an unwanted alias with
`<Using Remove="ActionsToolkit.Scripting.Core" />`, or use explicit qualified names.
Bare helpers are available through `using static ActionsToolkit.Scripting.Core;`.
Existing option/result types retain their original namespaces.
The GitHub module alias and fully qualified facade are
`GitHubApi` and `ActionsToolkit.Scripting.GitHubApi`, avoiding a collision
with the Octokit SDK's root `GitHub` namespace.

Use native `#:include helpers.cs` for shared declarations. Repository
`Directory.Build.*`, `Directory.Packages.props`, `NuGet.Config`, and `global.json`
still apply; the SDK does not disable consumer policy. Keep file-based apps outside
the compile cone of conventional projects.

Native AOT publishing defaults are preserved; normal execution does not require
publishing first. Use `#:property PublishAot=false` for an explicit managed
publishing opt-out. Custom JSON values require source-generated `JsonTypeInfo`.
Third-party packages still need their own AOT compatibility.

See the [showcase](../../samples/ActionsToolkit.Sdk.Sample/README.md) for host,
stdin, Docker, and step-to-step examples.
