// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// styling-output.cs
//
// Mirrors the upstream `@actions/core` README "Styling output" section:
// https://github.com/actions/toolkit/tree/main/packages/core#styling-output
//
// Colored output is supported via standard ANSI escape codes (3/4-bit,
// 8-bit, and 24-bit). Escape codes reset at the start of each line.
//
// Run with:
//   dotnet run styling-output.cs

#:package ActionsToolkit.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkit.Core.Extensions;
using ActionsToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

// Foreground colors.
core.WriteInfo("\u001b[35mThis foreground will be magenta");
core.WriteInfo("\u001b[38;5;6mThis foreground will be cyan");
core.WriteInfo("\u001b[38;2;255;0;0mThis foreground will be bright red");

// Background colors.
core.WriteInfo("\u001b[43mThis background will be yellow");
core.WriteInfo("\u001b[48;5;6mThis background will be cyan");
core.WriteInfo("\u001b[48;2;255;0;0mThis background will be bright red");

// Special styles.
core.WriteInfo("\u001b[1mBold text");
core.WriteInfo("\u001b[3mItalic text");
core.WriteInfo("\u001b[4mUnderlined text");

// Combined codes.
core.WriteInfo("\u001b[31;46mRed foreground with a cyan background and \u001b[1mbold text at the end");

// Escape codes reset at the start of each line.
core.WriteInfo("\u001b[35mThis foreground will be magenta");
core.WriteInfo("This foreground will reset to the default");
