// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Octokit.Common;

/// <summary>
/// Represents a GitHub issue.
/// </summary>
/// <param name="Owner">The owner of the issue. The first segment of this URL: <c>dotnet/runtime</c>, the owner would be <c>dotnet</c>.</param>
/// <param name="Repo">The repo of the issue. The second segment of this URL: <c>dotnet/runtime</c>, the owner would be <c>runtime</c>.</param>
/// <param name="Number">The issue number.</param>
public readonly record struct Issue(
    string Owner,
    string Repo,
    long Number);
