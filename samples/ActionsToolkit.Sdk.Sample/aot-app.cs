#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk
#:include helpers.cs

using static ActionsToolkit.Scripting.Core;

HashSet<string> configurations = [with(StringComparer.OrdinalIgnoreCase), "Release", "release"];
if (configurations.Count != 1)
    throw new InvalidOperationException("C# 15 comparer argument was not applied.");
Info(ShowcaseHelpers.Message);
await SetOutputAsync("message", ShowcaseHelpers.Message);
