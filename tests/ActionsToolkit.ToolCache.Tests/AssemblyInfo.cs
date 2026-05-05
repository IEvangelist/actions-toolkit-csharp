// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Xunit;

// Tests in this assembly mutate process-level environment variables
// (RUNNER_TOOL_CACHE / RUNNER_TEMP) via TempCacheFixture, so parallel
// execution would race. Disable assembly-wide test parallelization.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
