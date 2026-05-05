// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using var services = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = services.GetRequiredService<ICoreService>();

try
{
    // "who-to-greet" input defined in action metadata file
    var nameToGreet = core.GetInput("who-to-greet");
    core.WriteInfo($"Hello {nameToGreet}!");
    await core.SetOutputAsync("time", DateTime.UtcNow.ToString("o"));

    // Get the JSON webhook payload for the event that triggered the workflow
    var payload = Context.Current?.Payload?.ToString();

    core.WriteInfo($"The event payload: {payload}");

    await core.SetOutputAsync("yesItWorks", "testing/this/out");
}
catch (Exception ex)
{
    core.SetFailed(ex.ToString());
}
