// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// action-state.cs
//
// Mirrors the upstream `@actions/core` README "Action state" section:
// https://github.com/actions/toolkit/tree/main/packages/core#action-state
//
// SaveStateAsync persists state into $GITHUB_STATE so a `post:` step in
// the same wrapper action can read it back via GetState. State is scoped
// per action invocation and is not visible to other steps.
//
// Run with:
//   GITHUB_STATE=$(mktemp) STATE_PIDTOKILL=12345 dotnet run action-state.cs
//
// The STATE_* env var simulates the way the runner injects saved state
// back into the post step.

#:package ActionsToolkit.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.Core.Extensions;
using ActionsToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

// Main step: persist state for the post step.
await core.SaveStateAsync("pidToKill", 12345);

// Post step: retrieve previously saved state. The runner injects saved
// state as STATE_<NAME> environment variables.
var pid = core.GetState("pidToKill");
core.WriteInfo($"pidToKill (from STATE_PIDTOKILL): '{pid}'");
