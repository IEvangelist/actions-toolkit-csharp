// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// exit-codes.cs
//
// Mirrors the upstream `@actions/core` README "Exit codes" section:
// https://github.com/actions/toolkit/tree/main/packages/core#exit-codes
//
// SetFailed logs the message and sets a failing exit code. If the action
// runs to completion without calling SetFailed, the runner treats it as
// a success.
//
// Run with:
//   FAIL=1 dotnet run exit-codes.cs   # forces a failure path
//   dotnet run exit-codes.cs          # success path

#:package ActionsToolkitSharp.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Core.Extensions;
using ActionsToolkitSharp.Core.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

try
{
    if (Environment.GetEnvironmentVariable("FAIL") is not null)
    {
        throw new InvalidOperationException("Simulated failure (FAIL was set).");
    }

    core.WriteInfo("Doing work...");
}
catch (Exception ex)
{
    core.SetFailed($"Action failed with error {ex}");
}
