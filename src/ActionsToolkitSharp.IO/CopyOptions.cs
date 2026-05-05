// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.IO;

/// <summary>
/// Options object for copy <c>cp</c>.
/// </summary>
/// <param name="Recursive">
/// Whether to recursively copy all subdirectories.
/// Defaults to <c>false</c>.
/// </param>
/// <param name="Force">
/// Whether to overwrite existing files in the destination.
/// Defaults to <c>true</c>.
/// </param>
/// <param name="CopySourceDirectory">
/// Whether to copy the source directory along with all the files.
/// Only takes effect when recursive = true and copying a directory.
/// Defaults to <c>true</c>.
/// </param>
public readonly record struct CopyOptions(
    bool Recursive = false,
    bool Force = true,
    bool CopySourceDirectory = true)
{
    /// <summary>
    /// Creates a new instance of <see cref="CopyOptions"/>.
    /// </summary>
    public CopyOptions() : this(false, true, true) { }
}
