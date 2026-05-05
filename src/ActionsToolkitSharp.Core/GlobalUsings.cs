// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Diagnostics;
global using System.Globalization;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization.Metadata;

global using ActionsToolkitSharp.Core.Commands;
global using ActionsToolkitSharp.Core.Extensions;
global using ActionsToolkitSharp.Core.Markdown;
global using ActionsToolkitSharp.Core.Output;
global using ActionsToolkitSharp.Core.Services;
global using ActionsToolkitSharp.Core.Summaries;
global using ActionsToolkitSharp.Core.Workflows;

global using Microsoft.Extensions.DependencyInjection;

global using static System.Environment;
global using static System.IO.Path;

global using static ActionsToolkitSharp.Core.EnvironmentVariables.Keys;
global using static ActionsToolkitSharp.Core.EnvironmentVariables.Prefixes;
global using static ActionsToolkitSharp.Core.EnvironmentVariables.Suffixes;
