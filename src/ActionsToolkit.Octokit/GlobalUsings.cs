// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Diagnostics.CodeAnalysis;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using ActionsToolkit.Octokit.Interfaces;
global using ActionsToolkit.Octokit.Serialization;

global using GitHub;
global using GitHub.Octokit.Client.Authentication;
global using GitHub.Octokit.Client;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using static System.Environment;
global using static ActionsToolkit.Core.EnvironmentVariables.Keys;
