// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// set-secret.cs
//
// Mirrors the upstream `@actions/core` README "Setting a secret" section:
// https://github.com/actions/toolkit/tree/main/packages/core#setting-a-secret
//
// Registering a secret with the runner causes it to be masked from logs.
// Locally this just emits the workflow command `::add-mask::myPassword`
// to stdout — the runner does the actual masking at runtime.
//
// Run with:
//   dotnet run set-secret.cs

#:package ActionsToolkit.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.Core.Extensions;
using ActionsToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

core.SetSecret("myPassword");
core.WriteInfo("Registered 'myPassword' as a secret. The runner will mask it in logs.");
