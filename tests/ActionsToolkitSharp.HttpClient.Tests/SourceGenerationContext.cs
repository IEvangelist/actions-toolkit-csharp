// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.HttpClient.Tests;

[JsonSourceGenerationOptions(defaults: JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(PostmanEchoGetResponse))]
[JsonSerializable(typeof(PostmanEchoResponse))]
[JsonSerializable(typeof(RequestData))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}