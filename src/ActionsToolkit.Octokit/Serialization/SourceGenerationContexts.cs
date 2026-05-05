// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using CommonIssue = ActionsToolkit.Octokit.Common.Issue;
using CommonRepo = ActionsToolkit.Octokit.Common.Repository;
using Install = ActionsToolkit.Octokit.Interfaces.Installation;
using PR = ActionsToolkit.Octokit.Interfaces.PullRequest;

namespace ActionsToolkit.Octokit.Serialization;

[JsonSourceGenerationOptions(
    defaults: JsonSerializerDefaults.Web,
    WriteIndented = true,
    UseStringEnumConverter = true,
    AllowTrailingCommas = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    PropertyNameCaseInsensitive = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    IncludeFields = true)]
[JsonSerializable(typeof(CommonIssue))]
[JsonSerializable(typeof(CommonRepo))]
[JsonSerializable(typeof(Context))]
[JsonSerializable(typeof(Comment))]
[JsonSerializable(typeof(Install))]
[JsonSerializable(typeof(Owner))]
[JsonSerializable(typeof(PayloadRepository))]
[JsonSerializable(typeof(PR))]
[JsonSerializable(typeof(Sender))]
[JsonSerializable(typeof(WebhookIssue))]
[JsonSerializable(typeof(WebhookPayload))]
internal partial class SourceGenerationContexts : JsonSerializerContext
{
}
