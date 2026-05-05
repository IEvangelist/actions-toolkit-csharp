// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Marker collection for tests that mutate process-wide environment
/// variables (notably <c>GITHUB_SERVER_URL</c> and
/// <c>GITHUB_RETENTION_DAYS</c>). Members of this collection do not run in
/// parallel with each other, preventing inter-test interference.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "xUnit collection definitions are conventionally named with a 'Collection' suffix.")]
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class EnvironmentSensitiveCollection
{
    public const string Name = "Environment Sensitive";
}
