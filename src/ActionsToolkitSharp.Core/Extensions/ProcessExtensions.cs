// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Extensions;

internal static class ProcessExtensions
{
    internal static void GracefulOrForcedShutdown(this int pid)
    {
        Process.GetProcessById(pid)
            ?.GracefulOrForcedShutdown();
    }

    internal static void GracefulOrForcedShutdown(this Process process)
    {
        if (process is null)
        {
            return;
        }

        if (process.CloseMainWindow() is false)
        {
            process.WaitForExit(TimeSpan.FromSeconds(3));
        }

        while (process is not { HasExited: true })
        {
            process.Kill(true);
        }
    }
}
