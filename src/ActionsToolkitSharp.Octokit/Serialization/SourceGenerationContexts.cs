// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using CommonIssue = ActionsToolkitSharp.Octokit.Common.Issue;
using CommonRepo = ActionsToolkitSharp.Octokit.Common.Repository;
using Install = ActionsToolkitSharp.Octokit.Interfaces.Installation;
using PR = ActionsToolkitSharp.Octokit.Interfaces.PullRequest;

namespace ActionsToolkitSharp.Octokit.Serialization;

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
