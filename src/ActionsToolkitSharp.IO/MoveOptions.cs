// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.IO;

/// <summary>
/// Options object for move <c>mv</c>.
/// </summary>
/// <param name="Force">
/// Whether to overwrite existing files in the destination.
/// Defaults to <c>true</c>.
/// </param>
public readonly record struct MoveOptions(
    bool Force = true)
{
    /// <summary>
    /// Creates a new instance of <see cref="MoveOptions"/>.
    /// </summary>
    public MoveOptions() : this(true) { }
}
