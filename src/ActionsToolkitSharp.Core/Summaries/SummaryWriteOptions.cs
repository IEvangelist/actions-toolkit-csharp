// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Summaries;

/// <summary>
/// The options for writing the summary to the file.
/// </summary>
/// <param name="Overwrite">
/// (optional) Replace all existing content in summary file with buffer contents. Defaults to false.
/// </param>
public readonly record struct SummaryWriteOptions(bool Overwrite = false);
