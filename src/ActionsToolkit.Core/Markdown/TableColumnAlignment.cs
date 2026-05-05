// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Markdown;

/// <summary>
/// The alignment of the table columns. Use for either markdown or HTML tables.
/// </summary>
public enum TableColumnAlignment
{
    /// <summary>
    /// The default alignment for the table head. When writing markdown tables with the
    /// <see cref="Summary.AddMarkdownTable(SummaryTable)"/> API, results in "<c>---</c>".
    /// When writing HTML tables with the <see cref="Summary.AddTable(SummaryTableRow[])"/>
    /// API, results in no attributes.
    /// </summary>
    Center,

    /// <summary>
    /// The default alignment for the table head. When writing markdown tables with the
    /// <see cref="Summary.AddMarkdownTable(SummaryTable)"/> API, results in "<c>:--</c>".
    /// When writing HTML tables with the <see cref="Summary.AddTable(SummaryTableRow[])"/>
    /// API, results in <c>align="left"</c> attribute.
    /// </summary>
    Left,

    /// <summary>
    /// The default alignment for the table head. When writing markdown tables with the
    /// <see cref="Summary.AddMarkdownTable(SummaryTable)"/> API, results in "<c>--:</c>".
    /// When writing HTML tables with the <see cref="Summary.AddTable(SummaryTableRow[])"/>
    /// API, results in <c>align="right"</c> attribute.
    /// </summary>
    Right
}
