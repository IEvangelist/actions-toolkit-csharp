// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Net;
global using System.Net.Http;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;

global using ActionsToolkitSharp.Attest;
global using ActionsToolkitSharp.Attest.Auth;
global using ActionsToolkitSharp.Attest.Json;
global using ActionsToolkitSharp.Attest.Models;
global using ActionsToolkitSharp.Attest.Services;
global using ActionsToolkitSharp.Attest.Signing;
global using ActionsToolkitSharp.Attest.Store;

global using Microsoft.Extensions.DependencyInjection;

global using RichardSzalay.MockHttp;

global using Xunit;
