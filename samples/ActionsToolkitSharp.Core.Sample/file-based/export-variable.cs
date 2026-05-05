// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// export-variable.cs
//
// Mirrors the upstream `@actions/core` README "Exporting variables" section:
// https://github.com/actions/toolkit/tree/main/packages/core#exporting-variables
//
// Each step runs in a separate process. ExportVariableAsync writes a key/value
// pair into $GITHUB_ENV so the runner makes it visible to subsequent steps.
//
// Run with:
//   GITHUB_ENV=$(mktemp) dotnet run export-variable.cs

#:package ActionsToolkitSharp.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Core.Extensions;
using ActionsToolkitSharp.Core.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

await core.ExportVariableAsync("envVar", "Val");

core.WriteInfo("Wrote envVar=Val to $GITHUB_ENV.");
