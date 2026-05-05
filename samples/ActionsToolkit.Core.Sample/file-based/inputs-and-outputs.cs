// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// inputs-and-outputs.cs
//
// Mirrors the upstream `@actions/core` README "Inputs/Outputs" section:
// https://github.com/actions/toolkit/tree/main/packages/core#inputsoutputs
//
// Demonstrates:
//   * core.GetInput("name", new InputOptions(Required: true))
//   * core.GetBoolInput("name", new InputOptions(Required: true))
//   * core.GetMultilineInput("name", new InputOptions(Required: true))
//   * core.SetOutputAsync("key", "value") — writes to $GITHUB_OUTPUT
//
// Run with:
//   INPUT_INPUTNAME=hello \
//   INPUT_BOOLINPUTNAME=true \
//   INPUT_MULTILINEINPUTNAME=$'one\ntwo\nthree' \
//   GITHUB_OUTPUT=$(mktemp) \
//     dotnet run inputs-and-outputs.cs

#:package ActionsToolkit.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.Core;
using ActionsToolkit.Core.Extensions;
using ActionsToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

var myInput = core.GetInput("inputName", new InputOptions(Required: true));
var myBoolInput = core.GetBoolInput("boolInputName", new InputOptions(Required: true));
var myMultilineInput = core.GetMultilineInput("multilineInputName", new InputOptions(Required: true));

core.WriteInfo($"inputName        => {myInput}");
core.WriteInfo($"boolInputName    => {myBoolInput}");
core.WriteInfo($"multilineInputName ({myMultilineInput.Length}):");
foreach (var line in myMultilineInput)
{
    core.WriteInfo($"  - {line}");
}

await core.SetOutputAsync("outputKey", "outputVal");
