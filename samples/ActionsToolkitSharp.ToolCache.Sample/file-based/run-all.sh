#!/usr/bin/env bash
# Copyright (c) David Pine. All rights reserved.
# Licensed under the MIT License.
#
# Runs every file-based ActionsToolkitSharp.ToolCache example end-to-end.
# Each script provisions its own RUNNER_TEMP / RUNNER_TOOL_CACHE inside
# $TMPDIR, so this driver is fully idempotent.

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

# Per-script sandboxes so caches don't collide.
mk_sandbox() {
    local sandbox
    sandbox="$(mktemp -d)"
    mkdir -p "$sandbox/temp" "$sandbox/cache"
    echo "$sandbox"
}

s1=$(mk_sandbox)
RUNNER_TEMP="$s1/temp" RUNNER_TOOL_CACHE="$s1/cache" \
    run "evaluate-versions.cs" dotnet run evaluate-versions.cs

s2=$(mk_sandbox)
RUNNER_TEMP="$s2/temp" RUNNER_TOOL_CACHE="$s2/cache" \
    run "find.cs" dotnet run find.cs

s3=$(mk_sandbox)
RUNNER_TEMP="$s3/temp" RUNNER_TOOL_CACHE="$s3/cache" \
    run "manifest.cs" dotnet run manifest.cs

# `download-and-cache.cs` reaches the network — opt-in via env var, since the
# CI/PR drivers may not have outbound access.
if [[ "${RUN_NETWORK_SAMPLES:-false}" == "true" ]]; then
    s4=$(mk_sandbox)
    RUNNER_TEMP="$s4/temp" RUNNER_TOOL_CACHE="$s4/cache" \
        run "download-and-cache.cs" dotnet run download-and-cache.cs
else
    echo "[skip] download-and-cache.cs (set RUN_NETWORK_SAMPLES=true to run)"
fi

echo
echo "All ActionsToolkitSharp.ToolCache file-based examples executed."
