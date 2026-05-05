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

global using ActionsToolkit.HttpClient;
global using ActionsToolkit.HttpClient.Extensions;
global using ActionsToolkit.ToolCache;
global using ActionsToolkit.ToolCache.Extensions;
global using ActionsToolkit.ToolCache.Layout;
global using ActionsToolkit.ToolCache.Manifest;
global using ActionsToolkit.ToolCache.Retry;
global using ActionsToolkit.ToolCache.Semver;

global using Microsoft.Extensions.DependencyInjection;
