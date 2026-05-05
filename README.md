<!-- https://socialify.git.ci/IEvangelist/dotnet-github-actions-sdk?description=1&font=Rokkitt&language=1&name=1&owner=1&pattern=Plus&theme=Dark -->

![dotnet-github-actions-sdk](https://socialify.git.ci/IEvangelist/dotnet-github-actions-sdk/image?description=1&font=Rokkitt&language=1&name=1&owner=1&pattern=Plus&theme=Dark)

# GitHub Actions Workflow .NET SDK

[![build-and-test](https://github.com/IEvangelist/dotnet-github-actions-sdk/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/IEvangelist/dotnet-github-actions-sdk/actions/workflows/build-and-test.yml)
[![code analysis](https://github.com/IEvangelist/dotnet-github-actions-sdk/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/IEvangelist/dotnet-github-actions-sdk/actions/workflows/codeql-analysis.yml)
[![publish nuget](https://github.com/IEvangelist/dotnet-github-actions-sdk/actions/workflows/publish.yml/badge.svg)](https://github.com/IEvangelist/dotnet-github-actions-sdk/actions/workflows/publish.yml)
[![NuGet](https://img.shields.io/nuget/v/ActionsToolkitSharp.Core.svg?style=flat)](https://www.nuget.org/packages/ActionsToolkitSharp.Core) <!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-4-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

The .NET equivalent of the official GitHub [actions/toolkit](https://github.com/actions/toolkit) repository, and is currently a work in progress. While there isn't currently 100% feature complete compatibility between these two repositories, that is the eventual goal.

> [!IMPORTANT]
> This repository is in the middle of a rename and feature-parity initiative ([#5][issue-5]).
> NuGet package IDs are moving from `GitHub.Actions.*` to `ActionsToolkitSharp.*`, and all ten
> upstream `@actions/toolkit` packages will ship together as **v1.0.0** of `ActionsToolkitSharp.*`
> once each row in [PACKAGES.md](PACKAGES.md) is fully ✅. Native AOT correctness is verified per
> package via dedicated `tests/<pkg>.Aot.Tests` projects.

[issue-5]: https://github.com/IEvangelist/dotnet-github-actions-sdk/issues/5

## Blog

[🔗 Hello from the GitHub Actions: Core .NET SDK](https://davidpine.net/blog/github-actions-sdk)

## GitHub Actions .NET Toolkit

The GitHub Actions .NET ToolKit provides a set of packages to make creating actions easier.

## Packages

:heavy_check_mark: [`ActionsToolkitSharp.Core`](src/ActionsToolkitSharp.Core)

Provides functions for inputs, outputs, results, logging, secrets and variables. Read more [here](src/ActionsToolkitSharp.Core)

```
dotnet add package ActionsToolkitSharp.Core
```

For more information, see [📦 ActionsToolkitSharp.Core](https://www.nuget.org/packages/ActionsToolkitSharp.Core).

:ice_cream: [`ActionsToolkitSharp.Glob`](src/ActionsToolkitSharp.Glob)

Provides functions to search for files matching glob patterns. Read more [here](src/ActionsToolkitSharp.Glob)

```
dotnet add package ActionsToolkitSharp.Glob
```

For more information, see [📦 ActionsToolkitSharp.Glob](https://www.nuget.org/packages/ActionsToolkitSharp.Glob).

:pencil2: [`ActionsToolkitSharp.IO`](src/ActionsToolkitSharp.IO)

Provides disk i/o functions like cp, mv, rmRF, which etc. Read more [here](src/ActionsToolkitSharp.IO)

```bash
dotnet add package ActionsToolkitSharp.IO
```

For more information, see [📦 ActionsToolkitSharp.IO](https://www.nuget.org/packages/ActionsToolkitSharp.IO).

:octocat: [`ActionsToolkitSharp.Octokit`](src/ActionsToolkitSharp.Octokit)

Provides an Octokit client hydrated with the context that the current action is being run in. Read more [here](src/ActionsToolkitSharp.Octokit)

```bash
dotnet add package ActionsToolkitSharp.Octokit
```

For more information, see [📦 ActionsToolkitSharp.Octokit](https://www.nuget.org/packages/ActionsToolkitSharp.Octokit).

## Contributors ✨

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="http://www.chethusk.com"><img src="https://avatars.githubusercontent.com/u/573979?v=4?s=100" width="100px;" alt="Chet Husk"/><br /><sub><b>Chet Husk</b></sub></a><br /><a href="https://github.com/IEvangelist/dotnet-github-actions-sdk/commits?author=baronfel" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/js6pak"><img src="https://avatars.githubusercontent.com/u/35262707?v=4?s=100" width="100px;" alt="js6pak"/><br /><sub><b>js6pak</b></sub></a><br /><a href="https://github.com/IEvangelist/dotnet-github-actions-sdk/commits?author=js6pak" title="Code">💻</a> <a href="https://github.com/IEvangelist/dotnet-github-actions-sdk/commits?author=js6pak" title="Tests">⚠️</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://david.gardiner.net.au"><img src="https://avatars.githubusercontent.com/u/384747?v=4?s=100" width="100px;" alt="David Gardiner"/><br /><sub><b>David Gardiner</b></sub></a><br /><a href="https://github.com/IEvangelist/dotnet-github-actions-sdk/commits?author=flcdrg" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://thnetii.td.org.uit.no/"><img src="https://avatars.githubusercontent.com/u/8759693?v=4?s=100" width="100px;" alt="Fredrik Høisæther Rasch"/><br /><sub><b>Fredrik Høisæther Rasch</b></sub></a><br /><a href="https://github.com/IEvangelist/dotnet-github-actions-sdk/commits?author=fredrikhr" title="Code">💻</a> <a href="#ideas-fredrikhr" title="Ideas, Planning, & Feedback">🤔</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!
