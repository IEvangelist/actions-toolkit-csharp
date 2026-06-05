<!-- https://socialify.git.ci/IEvangelist/actions-toolkit-csharp?description=1&font=Rokkitt&language=1&name=1&owner=1&pattern=Plus&theme=Dark -->

![actions-toolkit-csharp](https://socialify.git.ci/IEvangelist/actions-toolkit-csharp/image?description=1&font=Rokkitt&language=1&name=1&owner=1&pattern=Plus&theme=Dark)

# GitHub Actions Workflow .NET SDK

[![build-and-test](https://github.com/IEvangelist/actions-toolkit-csharp/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/IEvangelist/actions-toolkit-csharp/actions/workflows/build-and-test.yml)
[![code analysis](https://github.com/IEvangelist/actions-toolkit-csharp/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/IEvangelist/actions-toolkit-csharp/actions/workflows/codeql-analysis.yml)
[![publish nuget](https://github.com/IEvangelist/actions-toolkit-csharp/actions/workflows/publish.yml/badge.svg)](https://github.com/IEvangelist/actions-toolkit-csharp/actions/workflows/publish.yml)
[![NuGet](https://img.shields.io/nuget/v/ActionsToolkit.Core.svg?style=flat)](https://www.nuget.org/packages/ActionsToolkit.Core) <!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-4-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

The .NET equivalent of the official GitHub [actions/toolkit](https://github.com/actions/toolkit) repository, and is currently a work in progress. While there isn't currently 100% feature complete compatibility between these two repositories, that is the eventual goal.

> [!IMPORTANT]
> This repository is in the middle of a rename and feature-parity initiative ([#5][issue-5]).
> NuGet package IDs are moving from `GitHub.Actions.*` to `ActionsToolkit.*`, and all ten
> upstream `@actions/toolkit` packages will ship together as **v1.0.0** of `ActionsToolkit.*`
> once each row in [PACKAGES.md](PACKAGES.md) is fully ✅. Native AOT correctness is verified per
> package via dedicated `tests/<pkg>.Aot.Tests` projects.

[issue-5]: https://github.com/IEvangelist/actions-toolkit-csharp/issues/5

## Blog

[🔗 Hello from the GitHub Actions: Core .NET SDK](https://davidpine.net/blog/github-actions-sdk)

## GitHub Actions .NET Toolkit

The GitHub Actions .NET ToolKit provides a set of packages to make creating actions easier.

## Packages

:heavy_check_mark: [`ActionsToolkit.Core`](src/ActionsToolkit.Core)

Provides functions for inputs, outputs, results, logging, secrets and variables. Read more [here](src/ActionsToolkit.Core)

```
dotnet add package ActionsToolkit.Core
```

For more information, see [📦 ActionsToolkit.Core](https://www.nuget.org/packages/ActionsToolkit.Core).

:ice_cream: [`ActionsToolkit.Glob`](src/ActionsToolkit.Glob)

Provides functions to search for files matching glob patterns. Read more [here](src/ActionsToolkit.Glob)

```
dotnet add package ActionsToolkit.Glob
```

For more information, see [📦 ActionsToolkit.Glob](https://www.nuget.org/packages/ActionsToolkit.Glob).

:pencil2: [`ActionsToolkit.IO`](src/ActionsToolkit.IO)

Provides disk i/o functions like cp, mv, rmRF, which etc. Read more [here](src/ActionsToolkit.IO)

```bash
dotnet add package ActionsToolkit.IO
```

For more information, see [📦 ActionsToolkit.IO](https://www.nuget.org/packages/ActionsToolkit.IO).

:rocket: [`ActionsToolkit.Exec`](src/ActionsToolkit.Exec)

Provides functions to exec command-line tools and process their output. Read more [here](src/ActionsToolkit.Exec)

```bash
dotnet add package ActionsToolkit.Exec
```

For more information, see [📦 ActionsToolkit.Exec](https://www.nuget.org/packages/ActionsToolkit.Exec).

:octocat: [`ActionsToolkit.Octokit`](src/ActionsToolkit.Octokit)

Provides an Octokit client hydrated with the context that the current action is being run in. Read more [here](src/ActionsToolkit.Octokit)

```bash
dotnet add package ActionsToolkit.Octokit
```

For more information, see [📦 ActionsToolkit.Octokit](https://www.nuget.org/packages/ActionsToolkit.Octokit).

:package: [`ActionsToolkit.ToolCache`](src/ActionsToolkit.ToolCache)

Provides functions for downloading, extracting, and caching tools (such as language runtimes) on the runner — the .NET equivalent of `@actions/tool-cache`. Read more [here](src/ActionsToolkit.ToolCache).

```bash
dotnet add package ActionsToolkit.ToolCache
```

For more information, see [📦 ActionsToolkit.ToolCache](https://www.nuget.org/packages/ActionsToolkit.ToolCache).

:lock: [`ActionsToolkit.Attest`](src/ActionsToolkit.Attest)

Provides functions for generating signed artifact attestations — Sigstore keyless signing of in-toto statements (including SLSA-style provenance) and persistence to GitHub's attestations REST endpoint. Read more [here](src/ActionsToolkit.Attest).

```bash
dotnet add package ActionsToolkit.Attest
```

For more information, see [📦 ActionsToolkit.Attest](https://www.nuget.org/packages/ActionsToolkit.Attest).

:floppy_disk: [`ActionsToolkit.Cache`](src/ActionsToolkit.Cache)

Provides functions for saving and restoring caches against the GitHub Actions cache service — the .NET equivalent of `@actions/cache`. Tar+zstd archive pipeline (managed via `System.Formats.Tar` + `ZstdSharp.Port`), Twirp+signed-URL transport for the V2 cache backend, and AOT-clean source-gen JSON. Read more [here](src/ActionsToolkit.Cache).

```bash
dotnet add package ActionsToolkit.Cache
```

For more information, see [📦 ActionsToolkit.Cache](https://www.nuget.org/packages/ActionsToolkit.Cache).

## Contributors ✨

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="http://www.chethusk.com"><img src="https://avatars.githubusercontent.com/u/573979?v=4?s=100" width="100px;" alt="Chet Husk"/><br /><sub><b>Chet Husk</b></sub></a><br /><a href="https://github.com/IEvangelist/actions-toolkit-csharp/commits?author=baronfel" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/js6pak"><img src="https://avatars.githubusercontent.com/u/35262707?v=4?s=100" width="100px;" alt="js6pak"/><br /><sub><b>js6pak</b></sub></a><br /><a href="https://github.com/IEvangelist/actions-toolkit-csharp/commits?author=js6pak" title="Code">💻</a> <a href="https://github.com/IEvangelist/actions-toolkit-csharp/commits?author=js6pak" title="Tests">⚠️</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://david.gardiner.net.au"><img src="https://avatars.githubusercontent.com/u/384747?v=4?s=100" width="100px;" alt="David Gardiner"/><br /><sub><b>David Gardiner</b></sub></a><br /><a href="https://github.com/IEvangelist/actions-toolkit-csharp/commits?author=flcdrg" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://thnetii.td.org.uit.no/"><img src="https://avatars.githubusercontent.com/u/8759693?v=4?s=100" width="100px;" alt="Fredrik Høisæther Rasch"/><br /><sub><b>Fredrik Høisæther Rasch</b></sub></a><br /><a href="https://github.com/IEvangelist/actions-toolkit-csharp/commits?author=fredrikhr" title="Code">💻</a> <a href="#ideas-fredrikhr" title="Ideas, Planning, & Feedback">🤔</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
# Proposed Improvements for GitHub Actions Workflow .NET SDK

## 🚀 Overview

This contribution improves the developer experience by enhancing documentation, providing real-world examples, and simplifying onboarding for new contributors.

---

## ✨ Added Features

### 1. Quick Start Guide

Added a beginner-friendly guide for creating a GitHub Action using the .NET SDK.

### 2. Real-World Examples

Added practical examples:

* Hello World Action
* Build & Test Action
* NuGet Package Publish Action
* Docker Build Action
* GitHub Release Automation

### 3. Architecture Section

Added architecture documentation explaining:

* ActionsToolkit.Core
* ActionsToolkit.Exec
* ActionsToolkit.IO
* ActionsToolkit.Cache
* ActionsToolkit.Octokit

and how they interact inside a GitHub Actions workflow.

### 4. Contributor Guide

Added step-by-step instructions for:

* Forking the repository
* Setting up the development environment
* Running tests
* Creating Pull Requests

### 5. FAQ Section

Added answers for common questions:

* Why use .NET instead of JavaScript?
* Is Native AOT supported?
* Which GitHub Actions features are implemented?
* How do I publish a custom action?

### 6. Sample Workflow Templates

Added reusable workflow examples for:

* Build
* Test
* Release
* Package Publishing

### 7. Documentation Improvements

Improved package descriptions and usage examples across the README.

---

## 📂 New Folder Structure

/examples
├── HelloWorldAction
├── BuildAction
├── DockerAction
├── NuGetPublishAction
└── ReleaseAutomation

/docs
├── QuickStart.md
├── Architecture.md
├── FAQ.md
└── ContributingGuide.md

/workflows
├── build.yml
├── test.yml
└── release.yml

---

## 🎯 Benefits

* Easier onboarding for new developers
* Better learning resources
* Faster adoption of the SDK
* Increased community contributions
* Improved project visibility
* Better open-source maintainability

---

## 🛠 Technologies

* C#
* .NET
* GitHub Actions
* NuGet
* Native AOT
* CI/CD
* Octokit

---

## 📈 Future Enhancements

* Full feature parity with @actions/toolkit
* Advanced caching support
* Artifact management
* Performance benchmarks
* More production-ready examples


This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!
