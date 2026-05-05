// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.IO.Tests;

public sealed class TempFolderTestFixture : IDisposable
{
    internal string TempFolder { get; } = Directory.CreateDirectory(
            path: Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        )
        .FullName;

    void IDisposable.Dispose()
    {
        Directory.Delete(TempFolder, true);
    }
}