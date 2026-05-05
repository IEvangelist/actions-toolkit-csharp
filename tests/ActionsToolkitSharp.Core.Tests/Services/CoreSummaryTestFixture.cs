// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Tests.Services;

public sealed class CoreSummaryTestFixture
{
    private readonly string TestDirectoryPath = Path.Combine(
               Directory.GetCurrentDirectory(), "test");

    internal readonly string TestFilePath = Path.Combine(
        Directory.GetCurrentDirectory(), "test", "test-summary.md");

    internal readonly TestCases TestCase = new();

    internal sealed record class TestCases
    {
        internal readonly string Text = "hello world 🌎";

        internal readonly string Code = """
            func fork() {
                for {
                    go fork()
                }
            }
            """;

        internal IEnumerable<string> List = ["foo", "bar", "baz", "💣"];

        internal IEnumerable<TaskItem> Tasks =
            [
                new TaskItem("foo"),
                new TaskItem("bar", true),
                new TaskItem("(Optional) baz"),
                new TaskItem("💣", false),
            ];

        internal SummaryTable SummaryTable = new(
            Heading: new SummaryTableRow(
                [
                        new("foo", Alignment: TableColumnAlignment.Right),
                        new("bar"),
                        new("baz", Alignment: TableColumnAlignment.Left),
                    ]),
            Rows: [
                new SummaryTableRow(
                    [
                    new("one"),
                    new("two"),
                    new("333"),
                ]),
                new SummaryTableRow(
                    [
                    new("a"),
                    new("b"),
                    new("c"),
                ])
            ]);

        internal SummaryTableRow[] Table =
            [
                new SummaryTableRow(
                    [
                        new("foo", true),
                        new("bar", true),
                        new("baz", true),
                        new("tall", false, Rowspan: 3),
                    ]),
                new SummaryTableRow(
                    [
                        new("one"),
                        new("two"),
                        new("three"),
                    ]),
                new SummaryTableRow(
                    [
                        new("wide", Colspan: 3),
                    ])
            ];

        internal (string Label, string Content) Details = ("open me", "🎉 surprise");

        internal (string Src, string Alt, SummaryImageOptions Options) Img =
            (
                "https://github.com/actions.png",
                "actions logo",
                new SummaryImageOptions(32, 32)
            );

        internal (string Text, string Cite) Quote = ("Where the world builds software", "https://github.com/about");

        internal (string Text, string Href) Link = ("GitHub", "https://github.com");
    }

    internal void Test(Action testBody)
    {
        try
        {
            BeforeEach();

            testBody.Invoke();
        }
        finally
        {
            AfterEach();
        }
    }

    internal async Task TestAsync(Func<Task> testBody)
    {
        try
        {
            BeforeEach();

            await testBody.Invoke();
        }
        finally
        {
            AfterEach();
        }
    }

    private void BeforeEach()
    {
        Environment.SetEnvironmentVariable(GITHUB_STEP_SUMMARY, TestFilePath);
        Directory.CreateDirectory(TestDirectoryPath);
        File.WriteAllText(TestFilePath, "");
    }

    private void AfterEach()
    {
        File.Delete(TestFilePath);
    }
}
