// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// job-summary.cs
//
// Mirrors the upstream `@actions/core` README "Populating job summary" section:
// https://github.com/actions/toolkit/tree/main/packages/core#populating-job-summary
//
// A job summary is a Markdown buffer attached to the job run. It is
// flushed to $GITHUB_STEP_SUMMARY by calling Summary.WriteAsync().
//
// Run with:
//   GITHUB_STEP_SUMMARY=$(mktemp) dotnet run job-summary.cs

#:package ActionsToolkitSharp.Core@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Core.Extensions;
using ActionsToolkitSharp.Core.Services;
using ActionsToolkitSharp.Core.Summaries;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsCore()
    .BuildServiceProvider();

var core = provider.GetRequiredService<ICoreService>();

core.Summary
    .AddHeading("My Heading", 2)
    .AddRaw("Some content here :speech_balloon:", addNewLine: true)
    .AddCodeBlock("Console.WriteLine(\"hello world\");", "csharp")
    .AddList(["item1", "item2", "item3"], ordered: true)
    .AddDetails("Label", "Some detail that will be collapsed")
    .AddSeparator()
    .AddQuote("To be or not to be", cite: "Shakespeare")
    .AddLink("click here", "https://github.com");

var headerRow = new SummaryTableRow(
[
    new SummaryTableCell("Header1", Header: true),
    new SummaryTableCell("Header2", Header: true),
    new SummaryTableCell("Header3", Header: true),
]);

var dataRow = new SummaryTableRow(
[
    new SummaryTableCell("MyData1"),
    new SummaryTableCell("MyData2"),
    new SummaryTableCell("MyData3"),
]);

core.Summary.AddTable([headerRow, dataRow]);

await core.Summary.WriteAsync(new SummaryWriteOptions(Overwrite: true));

core.WriteInfo("Wrote job summary to $GITHUB_STEP_SUMMARY.");
