#!/usr/bin/env bash
# Copyright (c) David Pine. All rights reserved.
# Licensed under the MIT License.
#
# Runs every file-based ActionsToolkitSharp.Octokit example end-to-end.
# Examples that need a real GitHub token are skipped automatically if
# GITHUB_TOKEN (or INPUT_MYTOKEN) is unset, so the driver is safe to
# invoke locally.

set -euo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$here"

run() {
    local title="$1"; shift
    echo
    echo "::group::${title}"
    "$@"
    echo "::endgroup::"
}

skip() {
    local title="$1" reason="$2"
    echo
    echo "::group::${title} (skipped)"
    echo "Skipped: ${reason}"
    echo "::endgroup::"
}

# ----------------------------------------------------------------------
# 1. Context
# ----------------------------------------------------------------------
run "context.cs" env \
    GITHUB_REPOSITORY="octocat/Hello-World" \
    GITHUB_REF="refs/heads/main" \
    GITHUB_SHA="ffac537e6cbbf934b08745a378932722df287a53" \
    GITHUB_WORKFLOW="demo" \
    GITHUB_EVENT_NAME="push" \
    GITHUB_ACTOR="octocat" \
    GITHUB_RUN_ID="1" \
    GITHUB_RUN_NUMBER="1" \
    GITHUB_RUN_ATTEMPT="1" \
    dotnet run context.cs

# ----------------------------------------------------------------------
# 2. Get a pull request via the hydrated client
# ----------------------------------------------------------------------
token="${INPUT_MYTOKEN:-${GITHUB_TOKEN:-}}"
if [[ -n "$token" ]]; then
    run "get-octokit.cs" env \
        INPUT_MYTOKEN="$token" \
        dotnet run get-octokit.cs
else
    skip "get-octokit.cs" "GITHUB_TOKEN / INPUT_MYTOKEN is not set"
fi

# ----------------------------------------------------------------------
# 3. Create an issue (dry-run by default — see the script body)
# ----------------------------------------------------------------------
if [[ -n "$token" && -n "${GITHUB_REPOSITORY:-}" ]]; then
    run "create-issue.cs (dry-run)" env \
        INPUT_MYTOKEN="$token" \
        GITHUB_REPOSITORY="$GITHUB_REPOSITORY" \
        dotnet run create-issue.cs
else
    skip "create-issue.cs" "GITHUB_TOKEN and GITHUB_REPOSITORY are required"
fi

echo
echo "All ActionsToolkitSharp.Octokit file-based examples executed."
