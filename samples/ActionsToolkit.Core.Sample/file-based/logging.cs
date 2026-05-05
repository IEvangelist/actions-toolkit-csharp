// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// logging.cs
//
// Mirrors the upstream `@actions/core` README "Logging" section:
// https://github.com/actions/toolkit/tree/main/packages/core#logging
//
// Demonstrates the full log-level surface:
//   * core.WriteDebug   — hidden by default; shown when RUNNER_DEBUG=1
//   * core.WriteInfo    — appears in the build log
//   * core.WriteNotice  — also produces an annotation
//   * core.WriteWarning — also produces an annotation
//   * core.WriteError   — also produces an annotation
//   * core.IsDebug      — branch on whether step debug logs are enabled
//
// Run with:
//   INPUT_INPUT=hello dotnet run logging.cs
//   RUNNER_DEBUG=1 INPUT_INPUT="" dotnet run logging.cs

#:package ActionsToolkit.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.Core.Extensions;
using ActionsToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

var myInput = core.GetInput("input");
try
{
    core.WriteDebug("Inside try block");

    if (string.IsNullOrEmpty(myInput))
    {
        core.WriteWarning("myInput was not set");
    }

    if (core.IsDebug)
    {
        core.WriteInfo("Verbose mode (RUNNER_DEBUG=1).");
    }
    else
    {
        core.WriteInfo("Standard mode.");
    }

    core.WriteInfo("Output to the actions build log");
    core.WriteNotice("This is a message that will also emit an annotation");
}
catch (Exception ex)
{
    core.WriteError($"Error {ex}, action may still succeed though");
}
