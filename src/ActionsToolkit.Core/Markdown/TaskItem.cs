// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Markdown;

/// <summary>
/// Represents a GitHub flavored markdown task item.
/// </summary>
/// <param name="Content">the content to render for the task</param>
/// <param name="IsComplete">(optional) whether the task is complete, default: <c>false</c></param>
public record class TaskItem(string Content, bool IsComplete = false);
