# Toolkit Packages

The following table tracks the various packages' development progress. Each package strives to provide functional equivalence with its corresponding `@actions/toolkit` package, but with the following additional features:

- **Testable**: The package is designed to be testable, with a clear separation between the core logic and the I/O operations. This allows for easier testing and mocking of the package's behavior.
- **DI friendly**: The package is designed to be dependency injection friendly, allowing for easier mocking and testing of the package's behavior.
- **README.md**: The package has a `README.md` file that describes its usage and behavior.
- **Tests**: The package has a test suite that validates its behavior.
- **Attribution**: The package has a clear attribution to the original `@actions/toolkit` package and any other 3rd party OSS packages that it depends on.
- **AOT-tested**: The package has a dedicated AOT consumer test project (`tests/<pkg>.Aot.Tests`) that publishes a tiny consumer with `PublishAot=true` + `TreatWarningsAsErrors=true` and asserts on its native binary output, proving the SDK is Native AOT-clean for end consumers.

> [!NOTE]
> The packages listed below are part of the in-progress rename to `ActionsToolkitSharp.*` ([#5][issue-5]). The previously published packages
> `GitHub.Actions.Core` and `GitHub.Actions.Glob` will receive a final v10.x release with `[Obsolete]` annotations,
> and the new `ActionsToolkitSharp.*` packages will ship together as **v1.0.0** once every row below is fully ✅.

[issue-5]: https://github.com/IEvangelist/dotnet-github-actions-sdk/issues/5

| `@actions/toolkit` | Package | Exists? | Testable? | DI Friendly? | README? | Tests? | Attribution? | AOT-tested? |
|--|--|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| `@actions/attest` | `ActionsToolkitSharp.Attest` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `@actions/cache` | `ActionsToolkitSharp.Cache` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `@actions/core` | `ActionsToolkitSharp.Core` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 🔳 |
| `@actions/artifact` | `ActionsToolkitSharp.Artifact` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `@actions/exec` | `ActionsToolkitSharp.Exec` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `@actions/github` | `ActionsToolkitSharp.Octokit` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 🔳 |
| `@actions/glob` | `ActionsToolkitSharp.Glob` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 🔳 |
| `@actions/http-client` | `ActionsToolkitSharp.HttpClient` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 🔳 |
| `@actions/io` | `ActionsToolkitSharp.IO` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 🔳 |
| `@actions/tool-cache` | `ActionsToolkitSharp.ToolCache` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

**Legend**

- **✅**: Done
- **🟡**: In progress (tracked by an open PR)
- **🔳**: Not started or not done

## Testable

Each package should strive to be testable, such that consumers can test all aspects of an API surface area with ease.

## DI Friendly

Going hand-in-hand with being testable, each package should strive to be dependency injection friendly, such that consumers register services via an `Add*` extension method on the `IServiceCollection` type.

## README.md

All packages require a `README.md` file that describes its usage and behavior. While they can be similar or derived from the original, it's best to keep these concise as they'll need to be packaged within the NuGet and link to a more thorough doc.

## Tests

All packages require a test suite that validates its behavior. This is a requirement for all packages, as it ensures that the package behaves as expected. Additionally, tests are a great way for consumers to learn how a bit of functionality is intended to behave.

The test layout mirrors the upstream `actions/toolkit` `__tests__/*.test.ts` file organization, with each upstream `it('…')` description appearing verbatim as a `[Fact(DisplayName="…")]` on the C# side. This preserves grep-from-upstream traceability when the upstream test suite evolves.

## Attribution

Each package is built atop various other packages, and it's important to give credit where credit is due. This includes the original `@actions/toolkit` package, as well as any other 3rd party OSS packages that the package depends on.

## AOT-tested

Each package ships a dedicated `tests/ActionsToolkitSharp.<Pkg>.Aot.Tests` project that:

1. Builds a tiny consumer console app with `<PublishAot>true</PublishAot>` + `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` + `<TrimmerSingleWarn>false</TrimmerSingleWarn>`.
2. Statically references every public API surface we want to validate from `Main` (the "dispatcher" pattern), so the trimmer roots all of them.
3. Publishes the consumer for the current RID, runs the resulting native binary with controlled environment variables, and asserts on stdout / stderr / exit code.

This catches IL2026 / IL3050 / IL3053 trim warnings, missing source-gen JSON contexts, dynamic reflection, and other AOT-incompatibilities that managed xUnit tests would never surface.
