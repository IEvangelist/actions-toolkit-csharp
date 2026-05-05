#!/usr/bin/env bash
# Copyright (c) David Pine. All rights reserved.
# Licensed under the MIT License.
#
# Runs every file-based ActionsToolkitSharp.Glob example end-to-end.
# Idempotent: each invocation works against the local working directory
# only and does not mutate any state.

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

run "basic.cs"     dotnet run basic.cs
run "recursive.cs" dotnet run recursive.cs
run "iterator.cs"  dotnet run iterator.cs

run "glob-with-input.cs (INPUT_FILES=**/*.cs)" \
    env INPUT_FILES="**/*.cs" \
    dotnet run glob-with-input.cs

run "builder.cs"   dotnet run builder.cs

echo
echo "All ActionsToolkitSharp.Glob file-based examples executed."
