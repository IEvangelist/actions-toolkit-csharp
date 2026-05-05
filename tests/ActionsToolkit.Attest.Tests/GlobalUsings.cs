// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Net;
global using System.Net.Http;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;

global using ActionsToolkit.Attest;
global using ActionsToolkit.Attest.Auth;
global using ActionsToolkit.Attest.Json;
global using ActionsToolkit.Attest.Models;
global using ActionsToolkit.Attest.Services;
global using ActionsToolkit.Attest.Signing;
global using ActionsToolkit.Attest.Store;

global using Microsoft.Extensions.DependencyInjection;

global using RichardSzalay.MockHttp;

global using Xunit;
