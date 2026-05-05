#!/usr/bin/env bash
# Copyright (c) David Pine. All rights reserved.
# Licensed under the MIT License.
#
# Runs every file-based ActionsToolkit.Core example end-to-end with
# the environment variables a real GitHub Actions runner would set, then
# echoes back the contents of each file-command file so you can see the
# side effects.
#
# Idempotent: each invocation creates its own scratch files via mktemp
# and cleans up afterwards.

set -euo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$here"

scratch="$(mktemp -d)"
trap 'rm -rf "$scratch"' EXIT

run() {
    local title="$1"; shift
    echo
    echo "::group::${title}"
    "$@"
    echo "::endgroup::"
}

dump() {
    local label="$1" path="$2"
    if [[ -s "$path" ]]; then
        echo
        echo "--- ${label} (${path}) ---"
        cat "$path"
        echo "--- end ${label} ---"
    fi
}

# ----------------------------------------------------------------------
# 1. Inputs and outputs
# ----------------------------------------------------------------------
GITHUB_OUTPUT="$scratch/github_output"
: > "$GITHUB_OUTPUT"

run "inputs-and-outputs.cs" env \
    INPUT_INPUTNAME="hello world" \
    INPUT_BOOLINPUTNAME="true" \
    INPUT_MULTILINEINPUTNAME=$'one\ntwo\nthree' \
    GITHUB_OUTPUT="$GITHUB_OUTPUT" \
    dotnet run inputs-and-outputs.cs

dump "GITHUB_OUTPUT" "$GITHUB_OUTPUT"

# ----------------------------------------------------------------------
# 2. Export variable
# ----------------------------------------------------------------------
GITHUB_ENV="$scratch/github_env"
: > "$GITHUB_ENV"

run "export-variable.cs" env \
    GITHUB_ENV="$GITHUB_ENV" \
    dotnet run export-variable.cs

dump "GITHUB_ENV" "$GITHUB_ENV"

# ----------------------------------------------------------------------
# 3. Set secret
# ----------------------------------------------------------------------
run "set-secret.cs" dotnet run set-secret.cs

# ----------------------------------------------------------------------
# 4. Add path
# ----------------------------------------------------------------------
GITHUB_PATH="$scratch/github_path"
: > "$GITHUB_PATH"

run "add-path.cs" env \
    GITHUB_PATH="$GITHUB_PATH" \
    dotnet run add-path.cs

dump "GITHUB_PATH" "$GITHUB_PATH"

# ----------------------------------------------------------------------
# 5. Exit codes (success path)
# ----------------------------------------------------------------------
run "exit-codes.cs (success)" dotnet run exit-codes.cs

# ----------------------------------------------------------------------
# 6. Logging
# ----------------------------------------------------------------------
run "logging.cs" env \
    INPUT_INPUT="hello" \
    dotnet run logging.cs

# ----------------------------------------------------------------------
# 7. Groups
# ----------------------------------------------------------------------
run "groups.cs" dotnet run groups.cs

# ----------------------------------------------------------------------
# 8. Annotations
# ----------------------------------------------------------------------
run "annotations.cs" dotnet run annotations.cs

# ----------------------------------------------------------------------
# 9. Styling output
# ----------------------------------------------------------------------
run "styling-output.cs" dotnet run styling-output.cs

# ----------------------------------------------------------------------
# 10. Action state
# ----------------------------------------------------------------------
GITHUB_STATE="$scratch/github_state"
: > "$GITHUB_STATE"

run "action-state.cs" env \
    GITHUB_STATE="$GITHUB_STATE" \
    STATE_PIDTOKILL="12345" \
    dotnet run action-state.cs

dump "GITHUB_STATE" "$GITHUB_STATE"

# ----------------------------------------------------------------------
# 11. Job summary
# ----------------------------------------------------------------------
GITHUB_STEP_SUMMARY="$scratch/github_step_summary"
: > "$GITHUB_STEP_SUMMARY"

run "job-summary.cs" env \
    GITHUB_STEP_SUMMARY="$GITHUB_STEP_SUMMARY" \
    dotnet run job-summary.cs

dump "GITHUB_STEP_SUMMARY" "$GITHUB_STEP_SUMMARY"

echo
echo "All ActionsToolkit.Core file-based examples executed."
