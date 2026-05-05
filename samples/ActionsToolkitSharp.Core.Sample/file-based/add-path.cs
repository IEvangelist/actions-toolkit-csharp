// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// add-path.cs
//
// Mirrors the upstream `@actions/core` README "PATH manipulation" section:
// https://github.com/actions/toolkit/tree/main/packages/core#path-manipulation
//
// AddPathAsync prepends a path to PATH for this and subsequent steps by
// writing to $GITHUB_PATH.
//
// Run with:
//   GITHUB_PATH=$(mktemp) dotnet run add-path.cs

#:package ActionsToolkitSharp.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Core.Extensions;
using ActionsToolkitSharp.Core.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

await core.AddPathAsync("/path/to/mytool");

core.WriteInfo("Prepended '/path/to/mytool' to $GITHUB_PATH.");
