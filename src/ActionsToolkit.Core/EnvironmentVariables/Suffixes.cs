// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.EnvironmentVariables;

/// <summary>
/// A collection of environment variable suffixes, used with corresponding <see cref="Prefixes"/>.
/// </summary>
#if ACTIONS_CORE_ENVIRONMENTVARIABLES_PUBLIC
public
#else
internal
#endif
static class Suffixes
{
    /// <summary>
    /// The environment variable key suffix: <c>ENV</c>.
    /// </summary>
    public const string ENV = nameof(ENV);
    /// <summary>
    /// The environment variable key suffix: <c>STATE</c>.
    /// </summary>
    public const string STATE = nameof(STATE);
    /// <summary>
    /// The environment variable key suffix: <c>OUTPUT</c>.
    /// </summary>
    public const string OUTPUT = nameof(OUTPUT);
}
