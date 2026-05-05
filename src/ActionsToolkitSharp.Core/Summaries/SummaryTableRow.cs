// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Summaries;

/// <summary>
/// Represents a row in a summary table.
/// </summary>
/// <param name="Cells">The cells for the row in context.</param>
public readonly record struct SummaryTableRow(
    SummaryTableCell[] Cells);
