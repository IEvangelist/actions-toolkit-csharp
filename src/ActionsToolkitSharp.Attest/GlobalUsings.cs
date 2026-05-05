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

global using ActionsToolkitSharp.Attest.Auth;
global using ActionsToolkitSharp.Attest.Json;
global using ActionsToolkitSharp.Attest.Models;
global using ActionsToolkitSharp.Attest.Services;
global using ActionsToolkitSharp.Attest.Signing;
global using ActionsToolkitSharp.Attest.Store;
global using ActionsToolkitSharp.Octokit;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using NetClient = System.Net.Http.HttpClient;
