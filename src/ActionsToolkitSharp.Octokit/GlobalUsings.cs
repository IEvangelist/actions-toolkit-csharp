// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Diagnostics.CodeAnalysis;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using ActionsToolkitSharp.Octokit.Interfaces;
global using ActionsToolkitSharp.Octokit.Serialization;

global using GitHub;
global using GitHub.Octokit.Client.Authentication;
global using GitHub.Octokit.Client;

global using Microsoft.Extensions.DependencyInjection;

global using static System.Environment;
global using static ActionsToolkitSharp.Core.EnvironmentVariables.Keys;
