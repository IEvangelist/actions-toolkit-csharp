#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

var name = Core.GetInput("name", required: true);
var enabled = Core.GetBoolInput("enabled", required: true);
var lines = Core.GetMultilineInput("items", required: true);
await Core.SetOutputAsync("name", name);
await Core.SetOutputAsync("enabled", enabled);
await Core.SetOutputAsync("items", lines.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
Core.Info($"Validated {name}: enabled={enabled}, {lines.Length} items");
