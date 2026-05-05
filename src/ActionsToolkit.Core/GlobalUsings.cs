// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Diagnostics;
global using System.Globalization;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization.Metadata;

global using ActionsToolkit.Core.Commands;
global using ActionsToolkit.Core.Extensions;
global using ActionsToolkit.Core.Markdown;
global using ActionsToolkit.Core.Output;
global using ActionsToolkit.Core.Services;
global using ActionsToolkit.Core.Summaries;
global using ActionsToolkit.Core.Workflows;

global using Microsoft.Extensions.DependencyInjection;

global using static System.Environment;
global using static System.IO.Path;

global using static ActionsToolkit.Core.EnvironmentVariables.Keys;
global using static ActionsToolkit.Core.EnvironmentVariables.Prefixes;
global using static ActionsToolkit.Core.EnvironmentVariables.Suffixes;
