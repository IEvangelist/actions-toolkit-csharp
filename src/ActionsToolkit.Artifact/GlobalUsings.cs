// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Buffers.Text;
global using System.IO.Compression;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.Json.Serialization.Metadata;

global using ActionsToolkit.Artifact.Internal;
global using ActionsToolkit.Artifact.Internal.Twirp;
global using ActionsToolkit.HttpClient;
global using ActionsToolkit.HttpClient.Extensions;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using NetClient = System.Net.Http.HttpClient;
