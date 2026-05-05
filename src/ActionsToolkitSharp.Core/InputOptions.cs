// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core;

/// <summary>
/// 
/// Inspired by <a href="https://github.com/actions/toolkit/blob/main/packages/core/src/core.ts"></a>
/// </summary>
/// <param name="Required">
/// Optional. Whether the input is required. If required and not present, will throw. Defaults to false.
/// </param>
/// <param name="TrimWhitespace">
/// Optional. Whether leading/trailing whitespace will be trimmed for the input. Defaults to true.
/// </param>
public readonly record struct InputOptions(
    bool Required = false,
    bool TrimWhitespace = true);