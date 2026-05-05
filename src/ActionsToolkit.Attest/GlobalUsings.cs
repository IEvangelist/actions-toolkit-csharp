// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Diagnostics.CodeAnalysis;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using System.Text.Json.Serialization.Metadata;

global using ActionsToolkit.Attest.Auth;
global using ActionsToolkit.Attest.Json;
global using ActionsToolkit.Attest.Models;
global using ActionsToolkit.Attest.Services;
global using ActionsToolkit.Attest.Signing;
global using ActionsToolkit.Attest.Store;
global using ActionsToolkit.Octokit;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using NetClient = System.Net.Http.HttpClient;
