#!/usr/bin/env bash
# Copyright (c) David Pine. All rights reserved.
# Licensed under the MIT License.
#
# Runs every file-based ActionsToolkitSharp.HttpClient example. The
# `get-todos.cs` script reaches out to jsonplaceholder.typicode.com, so
# this driver requires outbound network access. Set SKIP_NETWORK=1 to
# skip that step.

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

if [[ "${SKIP_NETWORK:-0}" == "1" ]]; then
    skip "get-todos.cs" "SKIP_NETWORK=1"
else
    run "get-todos.cs" dotnet run get-todos.cs
fi

run "bearer-token.cs" dotnet run bearer-token.cs

echo
echo "All ActionsToolkitSharp.HttpClient file-based examples executed."
