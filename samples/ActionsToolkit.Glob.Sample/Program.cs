// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();
var globber = Globber.Create(core.GetInput("files"));
foreach (var file in globber.GlobFiles())
{
    core.WriteInfo(file);
}