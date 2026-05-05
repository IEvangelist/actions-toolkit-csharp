// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Summaries;

/// <summary>
/// A job summary, see <a href="https://docs.github.com/actions/using-workflows/workflow-commands-for-github-actions#adding-a-job-summary"></a>.
/// </summary>
public sealed class Summary
{
    private enum Mode { Unspecified, Html, Markdown };

    private readonly StringBuilder _buffer = new();
    private string? _filePath;

    private Mode _previousMode = Mode.Unspecified;
    private Mode _currentMode = Mode.Unspecified;

    private static readonly string[] s_htmlHeadings = ["h1", "h2", "h3", "h4", "h5", "h6"];
    private static readonly string[] s_markdownHeadings = ["#", "##", "###", "####", "#####", "######"];

    /// <summary>If the summary buffer is empty</summary>
    /// <returns><c>true</c> if the buffer is empty</returns>
    public bool IsBufferEmpty => _buffer.Length is 0;

    /// <summary>
    /// Returns <c>true</c> if <c>GITHUB_STEP_SUMMARY</c> environment variable is set (as is the case when running in a GitHub Actions workflow).
    /// </summary>
    public static bool IsAvailable => GetEnvironmentVariable(GITHUB_STEP_SUMMARY) is not null;

    /// <summary>Finds the summary file path from the environment, rejects if env var 
    /// is not found or file does not exist. Also checks r/w permissions.</summary>
    /// <returns>The step summary file path</returns>
    private string FilePath()
    {
        if (_filePath is not null)
        {
            return _filePath;
        }

        var pathFromEnv = GetEnvironmentVariable(GITHUB_STEP_SUMMARY);

        if (string.IsNullOrWhiteSpace(pathFromEnv))
        {
            throw new ArgumentException($"""
                Unable to find environment variable for {GITHUB_STEP_SUMMARY}. 
                Check if your runtime environment supports job summaries.
                """);
        }

        return File.Exists(pathFromEnv)
            ? (_filePath = pathFromEnv)
            : throw new FileNotFoundException($"There is no file at: '{pathFromEnv}'");
    }

    /// <summary>Wraps content in an HTML tag, adding any HTML attributes,
    /// and sets the current mode as HTML.</summary>
    /// <param name="tag">tag HTML tag to wrap</param>
    /// <param name="content">content content within the tag</param>
    /// <param name="attributes">attributes key-value list of HTML attributes to add</param>
    /// <returns>A <c>string</c> content wrapped in HTML element</returns>
    private string Wrap(
        string tag,
        string? content,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        _currentMode = Mode.Html;

        var htmlAttributes = attributes?.Select(
            static kvp => $"{kvp.Key}=\"{kvp.Value}\"")
                ?.ToList();

        var attributeContent = htmlAttributes is { Count: > 0 }
            ? $" {string.Join(' ', htmlAttributes)}"
            : "";

        return content is null or { Length: 0 }
            ? $"<{tag}{attributeContent}>"
            : $"<{tag}{attributeContent}>{content}</{tag}>";
    }

    /// <summary>Writes text in the buffer to the summary buffer file and empties buffer. Will append by default.</summary>
    /// <param name="options">The (optional) <see cref="SummaryWriteOptions"/> for write operation.</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public async Task<Summary> WriteAsync(SummaryWriteOptions? options = default)
    {
        var path = FilePath();
        var mode = options is { Overwrite: true } ? FileMode.Truncate : FileMode.Append;

        await using var fs = new FileStream(path, mode);
        await using TextWriter writer = new StreamWriter(fs);

        var summaryContents = _buffer.ToString();

        if (mode is FileMode.Append && new FileInfo(path).Length > 0)
        {
            // Account for the potential of HTML to Markdown
            // (or vice versa) parsing mode change.
            await writer.WriteLineAsync();
            await writer.WriteLineAsync();
        }

        await writer.WriteAsync(summaryContents);

        await writer.FlushAsync();

        return EmptyBuffer();
    }

    /// <summary>Clears the summary buffer and wipes the summary file</summary>
    /// <returns>The <c>Summary</c> instance</returns>
    public Task<Summary> ClearAsync()
    {
        return EmptyBuffer().WriteAsync(new(true));
    }

    /// <summary>Returns the current summary buffer as a string</summary>
    /// <returns>A <c>string</c> representation of the summary buffer</returns>
    public string Stringify()
    {
        return _buffer.ToString();
    }

    /// <summary>Resets the summary buffer without writing to summary file</summary>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary EmptyBuffer()
    {
        _buffer.Clear();

        return this;
    }

    /// <summary>Adds a GitHub flavored markdown alert to the summary buffer</summary>
    /// <param name="content">the content to add to the alert</param>
    /// <param name="type">(optional) type of alert used to determine the rendering (default: Note)</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddAlert(string content, AlertType type = AlertType.Note)
    {
        return AddRawMarkdown($"""
            > [!{type.ToString().ToUpperInvariant()}]
            > {content}
            """, true);
    }

    /// <summary>Adds raw text to the summary buffer</summary>
    /// <param name="text">text content to add</param>
    /// <param name="addNewLine">(optional) append a <see cref="NewLine"/> to the raw text (default: false)</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddRaw(string text, bool addNewLine = false)
    {
        if (addNewLine)
        {
            // Switching from HTML to Markdown (or vice versa) requires two newlines.
            if (_currentMode != _previousMode && _previousMode is not Mode.Unspecified)
            {
                _buffer.Append(NewLine);
                _buffer.Append(NewLine);
            }

            _previousMode = _currentMode;

            _buffer.Append(text);
            _buffer.Append(NewLine);
        }
        else
        {
            _buffer.Append(text);
        }

        return this;
    }

    /// <summary>Adds raw markdown text to the summary buffer</summary>
    /// <param name="markdown">the markdown to add</param>
    /// <param name="addNewLine">(optional) append a <see cref="NewLine"/> to the raw text (default: false)</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddRawMarkdown(string markdown, bool addNewLine = false)
    {
        _currentMode = Mode.Markdown;

        return AddRaw(markdown, addNewLine);
    }

    /// <summary>Adds the operating system-specific end-of-line marker to the buffer</summary>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddNewLine()
    {
        return AddRaw(NewLine);
    }

    /// <summary>Adds an HTML codeblock to the summary buffer</summary>
    /// <param name="code">code content to render within fenced code block</param>
    /// <param name="lang">lang (optional) language to syntax highlight code</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddCodeBlock(string code, string? lang = null)
    {
        var attrs = lang is not null
            ? new Dictionary<string, string> { [nameof(lang)] = lang }
            : null;

        var element = Wrap("pre", Wrap("code", code), attrs);

        return AddRaw(element, true);
    }

    /// <summary>Adds a markdown codeblock to the summary buffer</summary>
    /// <param name="code">code content to render within fenced code block</param>
    /// <param name="lang">lang (optional) language to syntax highlight code</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddMarkdownCodeBlock(string code, string? lang = null)
    {
        var codeblock = $"""
            ```{lang}
            {code}
            ```
            """;

        return AddRawMarkdown(codeblock, true);
    }

    /// <summary>Adds an HTML list to the summary buffer</summary>
    /// <param name="items">items list of items to render</param>
    /// <param name="ordered">(optional) if the rendered list should be ordered or not (default: false)</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddList(IEnumerable<string> items, bool ordered = false)
    {
        var tag = ordered ? "ol" : "ul";
        var listItems = string.Join("", items.Select(item => Wrap("li", item)));
        var element = Wrap(tag, listItems);

        return AddRaw(element, true);
    }

    /// <summary>Adds a markdown list to the summary buffer</summary>
    /// <param name="items">items list of items to render</param>
    /// <param name="ordered">(optional) if the rendered list should be ordered or not (default: false)</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddMarkdownList(IEnumerable<string> items, bool ordered = false)
    {
        var tag = ordered ? "1." : "-";
        var listItems = string.Join(
            NewLine, items.Select(i => $"{tag} {i}"));

        return AddRawMarkdown(listItems, true);
    }

    /// <summary>Adds a markdown task list to the summary buffer</summary>
    /// <param name="items">items list of items to render</param>
    /// <returns>The <c>Summary</c> instance</returns>
    /// <remarks>
    /// See <a href="https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#task-lists"></a>
    /// </remarks>
    public Summary AddMarkdownTaskList(IEnumerable<TaskItem> items)
    {
        var listItems = string.Join(
            NewLine, items.Select(
                static i => $"- [{(i.IsComplete ? 'x' : ' ')}] {EscapeContent(i.Content)}"));

        return AddRawMarkdown(listItems, true);

        static string EscapeContent(string content)
        {
            if (content.StartsWith('('))
            {
                return $"\\{content}";
            }

            return content;
        }
    }

    /// <summary>Adds an HTML table to the summary buffer</summary>
    /// <param name="rows">rows table rows</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddTable(SummaryTableRow[] rows)
    {
        var tableBody = string.Join("", rows.Select(
            row =>
            {
                var cells = string.Join("", row.Cells.Select(
                    cell =>
                    {
                        if (cell.IsSimpleCell)
                        {
                            return Wrap("td", cell.Data);
                        }

                        var (data, header, colspan, rowspan, align) = cell;
                        var tag = header is true ? "th" : "td";

                        Dictionary<string, string>? attributes = null;

                        if (colspan.HasValue)
                        {
                            attributes ??= [];
                            attributes[nameof(colspan)] = $"{colspan.Value}";
                        }
                        if (rowspan.HasValue)
                        {
                            attributes ??= [];
                            attributes[nameof(rowspan)] = $"{rowspan.Value}";
                        }
                        if (align is not TableColumnAlignment.Center)
                        {
                            attributes ??= [];
                            attributes[nameof(align)] = align.ToString().ToLowerInvariant();
                        }

                        return Wrap(tag, data, attributes);
                    }));

                return Wrap("tr", cells);
            }));

        var element = Wrap("table", tableBody);
        return AddRaw(element, true);
    }

    /// <summary>Adds a markdown table to the summary buffer</summary>
    /// <param name="table">the table to write</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddMarkdownTable(SummaryTable table)
    {
        var heading = string.Join(" | ", table.Heading.Cells.Select(c => c.Data));
        var alignment = string.Join(" | ", table.Heading.Cells.Select(AlignmentSeparator));
        var body = string.Join("", table.Rows.Select((row, index) =>
        {
            var cells = string.Join(" | ", row.Cells.Select(c => c.Data));

            // Add a newline after all rows, except the last row
            var isLastRow = table.Rows.Length == index + 1;
            var end = isLastRow ? "" : NewLine;

            return $"| {cells} |{end}";
        }));

        var element = $"| {heading} |{NewLine}| {alignment} |{NewLine}{body}";

        return AddRawMarkdown(element, true);

        static string AlignmentSeparator(SummaryTableCell cell)
        {
            return cell switch
            {
                { Alignment: TableColumnAlignment.Left } => ":--",
                { Alignment: TableColumnAlignment.Right } => "--:",
                _ => "---"
            };
        }
    }

    /// <summary>Adds a collapsible HTML details element to the summary buffer</summary>
    /// <param name="label">label text for the closed state</param>
    /// <param name="content">content collapsible content</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddDetails(string label, string content)
    {
        var element = Wrap("details", $"{Wrap("summary", label)}{content}");

        return AddRaw(element, true);
    }

    /// <summary>Adds an HTML image tag to the summary buffer</summary>
    /// <param name="src">src path to the image you to embed</param>
    /// <param name="alt">alt text description of the image</param>
    /// <param name="options">(optional) addition image attributes</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddImage(string src, string alt, SummaryImageOptions? options = default)
    {
        var attributes = new Dictionary<string, string>
        {
            [nameof(src)] = src,
            [nameof(alt)] = alt
        };

        var (width, height) = options.GetValueOrDefault();
        if (width.HasValue)
        {
            attributes[nameof(width)] = $"{width.Value}";
        }
        if (height.HasValue)
        {
            attributes[nameof(height)] = $"{height.Value}";
        }

        var element = Wrap("img", null, attributes);
        return AddRaw(element, true);
    }

    /// <summary>Adds an HTML section heading element</summary>
    /// <param name="text">text heading text</param>
    /// <param name="level">(optional) the heading level, default: 1</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddHeading(string text, int level = 1)
    {
        var tag = $"h{level}";
        var allowedTag = s_htmlHeadings.Any(h => h == tag)
            ? tag : "h1";
        var element = Wrap(allowedTag, text);
        return AddRaw(element, true);
    }

    /// <summary>Adds a markdown section heading element</summary>
    /// <param name="text">text heading text</param>
    /// <param name="level">(optional) the heading level, default: 1</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddMarkdownHeading(string text, int level = 1)
    {
        level = Math.Max(1, level);
        var heading = new string('#', level);
        var allowedHeading = s_markdownHeadings.Any(h => h == heading)
            ? heading : "#";
        var element = $"{allowedHeading} {text}";
        return AddRawMarkdown(element, true);
    }

    /// <summary>Adds an HTML thematic break "<c>&lt;hr&gt;</c>" to the summary buffer</summary>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddSeparator()
    {
        return AddRaw("<hr>", true);
    }

    /// <summary>Adds a markdown thematic break "<c>---</c>" to the summary buffer</summary>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddMarkdownSeparator()
    {
        return AddRawMarkdown("---", true);
    }

    /// <summary>Adds an HTML line break "<c>&lt;br&gt;</c>" to the summary buffer</summary>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddBreak()
    {
        return AddRaw("<br>", true);
    }

    /// <summary>Adds an HTML blockquote to the summary buffer</summary>
    /// <param name="text">quote text</param>
    /// <param name="cite">cite (optional) citation url</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddQuote(string text, string? cite = null)
    {
        var attributes = cite is not null
            ? new Dictionary<string, string>
            {
                [nameof(cite)] = cite
            }
            : null;
        var element = Wrap("blockquote", text, attributes);
        return AddRaw(element, true);
    }

    /// <summary>Adds an markdown quote to the summary buffer</summary>
    /// <param name="text">quote text</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddMarkdownQuote(string text)
    {
        var element = $"> {text}";

        return AddRawMarkdown(element, true);
    }

    /// <summary>Adds an HTML anchor tag to the summary buffer</summary>
    /// <param name="text">text link text/content</param>
    /// <param name="href">href hyperlink</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddLink(string text, string href)
    {
        var element = Wrap("a", text, new Dictionary<string, string>() { [nameof(href)] = href });

        return AddRaw(element, true);
    }

    /// <summary>Adds a markdown link to the summary buffer</summary>
    /// <param name="text">text link text/content</param>
    /// <param name="href">href hyperlink</param>
    /// <returns>The <c>Summary</c> instance</returns>
    public Summary AddMarkdownLink(string text, string href)
    {
        var element = $"[{text}]({href})";

        return AddRawMarkdown(element, true);
    }
}
