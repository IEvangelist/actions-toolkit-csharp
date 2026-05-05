# Tests

This folder contains the test projects for the ActionsToolkitSharp packages.

## Layout

Each package has up to two test projects, both following a strict naming
convention so CI can discover them automatically:

| Convention                                         | Purpose                                                       |
| -------------------------------------------------- | ------------------------------------------------------------- |
| `tests/ActionsToolkitSharp.<Package>.Tests/`       | Managed (CoreCLR) unit and integration tests for `<Package>`. |
| `tests/ActionsToolkitSharp.<Package>.Aot.Tests/`   | NativeAOT-compatible smoke tests for `<Package>`.             |

The shared `AotTestSupport/` project provides helpers for the AOT smoke tests
and is not itself a test project.

## How CI discovers test projects

The `build-and-test` workflow (and the shared `./.github/actions/build`
composite action) runs `dotnet test` against the repository, which uses
[`ActionsToolkitSharp.sln`](../ActionsToolkitSharp.sln) as the source of
truth for the projects to build and execute.

That means **the solution is the discovery mechanism**. Any test project
that lives on disk but is not registered in the `.sln` would be silently
skipped by CI.

## Adding a new test project

When you add a new package (for example `ActionsToolkitSharp.Foo`):

1. Create the test project(s) under `tests/`, following the naming
   convention above:
   - `tests/ActionsToolkitSharp.Foo.Tests/ActionsToolkitSharp.Foo.Tests.csproj`
   - (Optionally) `tests/ActionsToolkitSharp.Foo.Aot.Tests/ActionsToolkitSharp.Foo.Aot.Tests.csproj`

2. Register the new project(s) in the solution so CI picks them up:

   ```bash
   dotnet sln ActionsToolkitSharp.sln add tests/ActionsToolkitSharp.Foo.Tests/ActionsToolkitSharp.Foo.Tests.csproj
   ```

3. **No workflow edit is required.** The `build-and-test` workflow has a
   `verify-test-projects` job that fails the build if any
   `tests/ActionsToolkitSharp.*.Tests/*.csproj` file exists on disk but is
   not referenced from `ActionsToolkitSharp.sln`, which keeps this
   convention enforced going forward.
