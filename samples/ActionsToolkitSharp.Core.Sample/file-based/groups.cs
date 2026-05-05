// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// groups.cs
//
// Mirrors the upstream `@actions/core` README "foldable groups" snippet:
// https://github.com/actions/toolkit/tree/main/packages/core#logging
//
// Demonstrates two ways to wrap output in a foldable group:
//   * Manual: core.StartGroup / core.EndGroup
//   * Async wrapper: core.GroupAsync(name, () => ValueTask<T> ...)
//
// Run with:
//   dotnet run groups.cs

#:package ActionsToolkitSharp.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Core.Extensions;
using ActionsToolkitSharp.Core.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

// Manually wrap output.
core.StartGroup("Do some function");
SomeFunction(core);
core.EndGroup();

// Wrap an asynchronous function call.
var result = await core.GroupAsync("Do something async", async () =>
{
    var response = await MakeHttpRequestAsync();
    return response;
});

core.WriteInfo($"Async result: {result}");

static void SomeFunction(ICoreService core)
{
    core.WriteInfo("Step 1");
    core.WriteInfo("Step 2");
    core.WriteInfo("Step 3");
}

static async ValueTask<string> MakeHttpRequestAsync()
{
    await Task.Delay(25);
    return "ok";
}
