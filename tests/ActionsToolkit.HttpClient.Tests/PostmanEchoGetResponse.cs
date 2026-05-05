// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient.Tests;

public record class PostmanEchoGetResponse(
    Args Args,
    Dictionary<string, string> Headers,
    string Url);
