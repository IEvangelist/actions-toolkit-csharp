// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.EnvironmentVariables;

/// <summary>
/// A collection of environment variable prefixes, used with corresponding <see cref="Suffixes"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1707:Identifiers should not contain underscores",
    Justification = "These values correspond to environment variables and I want them to match exactly.")]
#if ACTIONS_CORE_ENVIRONMENTVARIABLES_PUBLIC
public
#else
internal
#endif
static class Prefixes
{
    /// <summary>
    /// The environment variable key prefix: <c>GITHUB_</c>.
    /// </summary>
    public const string GITHUB_ = nameof(GITHUB_);
    /// <summary>
    /// The environment variable key prefix: <c>INPUT_</c>.
    /// </summary>
    public const string INPUT_ = nameof(INPUT_);
    /// <summary>
    /// The environment variable key prefix: <c>STATE_</c>.
    /// </summary>
    public const string STATE_ = nameof(STATE_);
}
