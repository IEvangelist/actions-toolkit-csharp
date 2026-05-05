// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// context.cs
//
// Mirrors the upstream `@actions/github` README "context" snippet:
// https://github.com/actions/toolkit/tree/main/packages/github
//
// `Context.Current` is materialized from the runner's environment
// variables (GITHUB_REPOSITORY, GITHUB_REF, GITHUB_SHA, GITHUB_WORKFLOW,
// GITHUB_EVENT_PATH, ...). Use it to discover what triggered the
// workflow and which repository/branch the action is operating on.
//
// Run with:
//   GITHUB_REPOSITORY=octocat/Hello-World \
//   GITHUB_REF=refs/heads/main \
//   GITHUB_SHA=ffac537e6cbbf934b08745a378932722df287a53 \
//   GITHUB_WORKFLOW=demo \
//   GITHUB_EVENT_NAME=push \
//   GITHUB_RUN_ID=1 GITHUB_RUN_NUMBER=1 GITHUB_RUN_ATTEMPT=1 \
//   dotnet run context.cs

#:package ActionsToolkit.Octokit@*

using ActionsToolkit.Octokit;

var context = Context.Current;

Console.WriteLine($"Workflow:  {context.Workflow}");
Console.WriteLine($"EventName: {context.EventName}");
Console.WriteLine($"Actor:     {context.Actor}");
Console.WriteLine($"Sha:       {context.Sha}");
Console.WriteLine($"Ref:       {context.Ref}");
Console.WriteLine($"Repo:      {context.Repo.Owner}/{context.Repo.Repo}");
Console.WriteLine($"RunId:     {context.RunId}");
Console.WriteLine($"RunNumber: {context.RunNumber}");
Console.WriteLine($"ApiUrl:    {context.ApiUrl}");
Console.WriteLine($"ServerUrl: {context.ServerUrl}");
