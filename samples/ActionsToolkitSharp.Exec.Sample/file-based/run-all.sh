#!/usr/bin/env bash
# Copyright (c) David Pine. All rights reserved.
# Licensed under the MIT License.
#
# Runs every file-based ActionsToolkitSharp.Exec example end-to-end. Each
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

run "exec-basic.cs"    dotnet run exec-basic.cs
run "get-output.cs"    dotnet run get-output.cs
run "with-listeners.cs" dotnet run with-listeners.cs
run "with-env.cs"      dotnet run with-env.cs
run "with-cwd.cs"      dotnet run with-cwd.cs

echo
echo "All ActionsToolkitSharp.Exec file-based examples executed."
