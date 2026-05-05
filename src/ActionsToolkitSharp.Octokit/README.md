# `ActionsToolkitSharp.Octokit` package

To install the [`ActionsToolkitSharp.Octokit`](https://www.nuget.org/packages/ActionsToolkitSharp.Octokit) NuGet package:

```xml
<PackageReference Include="ActionsToolkitSharp.Octokit" Version="[Version]" />
```

Or use the [`dotnet add package`](https://learn.microsoft.com/dotnet/core/tools/dotnet-add-package) .NET CLI command:

```bash
dotnet add package ActionsToolkitSharp.Octokit
```

## `ActionsToolkitSharp.Octokit`

This was modified, but borrowed from the [_glob/README.md_](https://github.com/actions/toolkit/blob/main/packages/github/README.md).

> You can use this package to access a hydrated Octokit client with authentication and a set of useful defaults for GitHub Actions.

### Get the `GitHubClient` instance

To use the `GitHubClient` in your .NET project, register the services with an `IServiceCollection` instance by calling `AddGitHubClientServices` and then your consuming code can require the `GitHubClient` via constructor dependency injection.

```csharp
using Microsoft.Extensions.DependencyInjection;
using GitHub;
using ActionsToolkitSharp.Octokit;
using ActionsToolkitSharp.Octokit.Extensions;

using var provider = new ServiceCollection()
    .AddGitHubClientServices()
    .BuildServiceProvider();

// The client relies on the value from ${{ secrets.GITHUB_TOKEN }}
var client = provider.GetRequiredService<GitHubClient>();

// Call GitHub REST API /repos/octokit/rest.js/pulls/123
var pullRequest = client.Repos["octokit"]["rest.js"].Pulls[123].GetAsync();

Console.WriteLine(pullRequest.Title);
```
