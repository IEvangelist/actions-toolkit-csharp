# Tests

This folder contains the test projects for the ActionsToolkit packages.

## Layout

Each package has up to two test projects, both following a strict naming
convention so CI can discover them automatically:

| Convention                                         | Purpose                                                       |
| -------------------------------------------------- | ------------------------------------------------------------- |
| `tests/ActionsToolkit.<Package>.Tests/`       | Managed (CoreCLR) unit and integration tests for `<Package>`. |
| `tests/ActionsToolkit.<Package>.Aot.Tests/`   | NativeAOT-compatible smoke tests for `<Package>`.             |

The shared `AotTestSupport/` project provides helpers for the AOT smoke tests
and is not itself a test project.

## How CI discovers test projects

The `build-and-test` workflow (and the shared `./.github/actions/build`
composite action) runs `dotnet test` against the repository, which uses
[`ActionsToolkit.slnx`](../ActionsToolkit.slnx) as the source of
truth for the projects to build and execute.

That means **the solution is the discovery mechanism**. Any test project
that lives on disk but is not registered in the `.slnx` would be silently
skipped by CI.

## Adding a new test project

When you add a new package (for example `ActionsToolkit.Foo`):

1. Create the test project(s) under `tests/`, following the naming
   convention above:
   - `tests/ActionsToolkit.Foo.Tests/ActionsToolkit.Foo.Tests.csproj`
   - (Optionally) `tests/ActionsToolkit.Foo.Aot.Tests/ActionsToolkit.Foo.Aot.Tests.csproj`

2. Register the new project(s) in the solution so CI picks them up:

   ```bash
   dotnet sln ActionsToolkit.slnx add tests/ActionsToolkit.Foo.Tests/ActionsToolkit.Foo.Tests.csproj
   ```

3. **No workflow edit is required.** The `build-and-test` workflow has a
   `verify-test-projects` job that fails the build if any
   `tests/ActionsToolkit.*.Tests/*.csproj` file exists on disk but is
   not referenced from `ActionsToolkit.slnx`, which keeps this
   convention enforced going forward.
