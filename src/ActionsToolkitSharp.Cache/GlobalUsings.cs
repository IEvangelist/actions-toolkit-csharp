// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Buffers;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.Json.Serialization.Metadata;

global using ActionsToolkitSharp.Cache.Errors;
global using ActionsToolkitSharp.Cache.Internal;
global using ActionsToolkitSharp.Cache.Internal.Twirp;
global using ActionsToolkitSharp.Cache.Json;
global using ActionsToolkitSharp.Cache.Models;
global using ActionsToolkitSharp.Cache.Services;
global using ActionsToolkitSharp.HttpClient;
global using ActionsToolkitSharp.HttpClient.Extensions;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using NetClient = System.Net.Http.HttpClient;
