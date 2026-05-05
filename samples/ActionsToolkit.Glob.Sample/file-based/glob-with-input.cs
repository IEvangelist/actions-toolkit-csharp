// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// glob-with-input.cs
//
// Mirrors the upstream `@actions/glob` README "Recommended action inputs"
// section, where a user-supplied `files` input drives the glob:
// https://github.com/actions/toolkit/tree/main/packages/glob#recommended-action-inputs
//
// Combines `ActionsToolkit.Core` (to read the input) with
// `ActionsToolkit.Glob` (to resolve the matching files).
//
// Run with:
//   INPUT_FILES="**/*.cs" dotnet run glob-with-input.cs

#:package ActionsToolkit.Core@*
#:package ActionsToolkit.Glob@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.Core.Extensions;
using ActionsToolkit.Core.Services;
using ActionsToolkit.Glob;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .AddGitHubActionsGlob()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

var pattern = core.GetInput("files");
if (string.IsNullOrWhiteSpace(pattern))
{
    core.SetFailed("The 'files' input is required.");
    return;
}

var globber = Globber.Create(pattern);
foreach (var file in globber.GlobFiles())
{
    core.WriteInfo(file);
}
