#!/usr/bin/env bash
# Copyright (c) David Pine. All rights reserved.
# Licensed under the MIT License.
#
# Runs every file-based ActionsToolkitSharp.IO example end-to-end. Each
# script uses Path.GetTempPath() for its scratch directories and cleans
# up after itself, so this driver is fully idempotent.

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

run "mkdir.cs" dotnet run mkdir.cs
run "cp-mv.cs" dotnet run cp-mv.cs
run "rm-rf.cs" dotnet run rm-rf.cs
run "which.cs" dotnet run which.cs

echo
echo "All ActionsToolkitSharp.IO file-based examples executed."
