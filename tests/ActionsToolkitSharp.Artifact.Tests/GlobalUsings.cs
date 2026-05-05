// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Net;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Text;
global using System.IO.Compression;

global using Xunit;

global using ActionsToolkitSharp.Artifact;
global using ActionsToolkitSharp.Artifact.Internal;
global using ActionsToolkitSharp.Artifact.Internal.Twirp;
global using ActionsToolkitSharp.HttpClient;

global using Microsoft.Extensions.DependencyInjection;
