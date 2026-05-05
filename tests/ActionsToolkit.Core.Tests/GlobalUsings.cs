// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using ActionsToolkit.Core.Commands;
global using ActionsToolkit.Core.Extensions;
global using ActionsToolkit.Core.Markdown;
global using ActionsToolkit.Core.Output;
global using ActionsToolkit.Core.Services;
global using ActionsToolkit.Core.Summaries;
global using ActionsToolkit.Core.Tests.Output;
global using ActionsToolkit.Core.Workflows;

global using ActionsToolkit.Core.EnvironmentVariables;

global using Microsoft.Extensions.DependencyInjection;

global using static ActionsToolkit.Core.EnvironmentVariables.Keys;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1859:Use concrete types when possible for improved performance",
    Justification = "<Pending>")
]
