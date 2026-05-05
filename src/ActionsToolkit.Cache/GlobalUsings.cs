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

global using ActionsToolkit.Cache.Errors;
global using ActionsToolkit.Cache.Internal;
global using ActionsToolkit.Cache.Internal.Twirp;
global using ActionsToolkit.Cache.Json;
global using ActionsToolkit.Cache.Models;
global using ActionsToolkit.Cache.Services;
global using ActionsToolkit.HttpClient;
global using ActionsToolkit.HttpClient.Extensions;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using NetClient = System.Net.Http.HttpClient;
