// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// The pair of opaque identifiers that the GitHub Actions results service
/// requires on every Twirp RPC. They are encoded into the
/// <c>Actions.Results</c> scope claim of the <c>ACTIONS_RUNTIME_TOKEN</c> JWT
/// in the form <c>Actions.Results:&lt;workflow_run&gt;:&lt;workflow_job_run&gt;</c>.
/// </summary>
/// <param name="WorkflowRunBackendId">The backend identifier of the current
/// workflow run.</param>
/// <param name="WorkflowJobRunBackendId">The backend identifier of the
/// current workflow job run.</param>
internal sealed record BackendIds(
    string WorkflowRunBackendId,
    string WorkflowJobRunBackendId);
