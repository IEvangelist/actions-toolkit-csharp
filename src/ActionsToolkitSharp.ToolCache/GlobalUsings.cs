// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.IO.Compression;
global using System.Net;
global using System.Runtime.InteropServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.Json.Serialization.Metadata;
global using System.Text.RegularExpressions;

global using ActionsToolkitSharp.HttpClient;
global using ActionsToolkitSharp.HttpClient.Extensions;
global using ActionsToolkitSharp.ToolCache;
global using ActionsToolkitSharp.ToolCache.Extensions;
global using ActionsToolkitSharp.ToolCache.Layout;
global using ActionsToolkitSharp.ToolCache.Manifest;
global using ActionsToolkitSharp.ToolCache.Retry;
global using ActionsToolkitSharp.ToolCache.Semver;

global using Microsoft.Extensions.DependencyInjection;
