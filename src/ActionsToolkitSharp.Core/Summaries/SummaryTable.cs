// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Summaries;

/// <summary>
/// Represents a table in a summary. Each row must have the same number of columns.
/// Only simple cells (cells with just their <see cref="SummaryTableCell.Data"/>
/// values populated) are supported.
/// </summary>
/// <param name="Heading">The row used for the heading</param>
/// <param name="Rows">The rows in the table.</param>
public readonly record struct SummaryTable(
    SummaryTableRow Heading,
    SummaryTableRow[] Rows);
