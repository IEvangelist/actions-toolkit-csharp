// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// annotations.cs
//
// Mirrors the upstream `@actions/core` README "Annotations" section:
// https://github.com/actions/toolkit/tree/main/packages/core#annotations
//
// WriteError / WriteWarning / WriteNotice each accept optional
// AnnotationProperties so the annotation can target a specific file,
// line, and column in the rendered Actions UI.
//
// Run with:
//   dotnet run annotations.cs

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

core.WriteError(
    "This is a bad error, action may still succeed though.");

core.WriteWarning(
    "Something went wrong, but it's not bad enough to fail the build.");

core.WriteNotice(
    "Something happened that you might want to know about.");

// Annotations attached to a particular file/line/column.
var properties = new AnnotationProperties
{
    Title = "Suspicious code",
    File = "src/MyApp/Program.cs",
    StartLine = 42,
    EndLine = 42,
    StartColumn = 9,
    EndColumn = 17,
};

core.WriteWarning("Variable is never used", properties);
