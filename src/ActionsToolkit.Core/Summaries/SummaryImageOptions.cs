// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Summaries;

/// <summary>
/// The options for the summary image.
/// </summary>
/// <param name="Width">(optional) The width of the image in pixels. Must be an integer without a unit.</param>
/// <param name="Height">(optional) The height of the image in pixels. Must be an integer without a unit.</param>
public readonly record struct SummaryImageOptions(
    int? Width = null,
    int? Height = null);
