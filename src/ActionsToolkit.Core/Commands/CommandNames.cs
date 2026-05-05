// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Commands;

internal static class CommandNames
{
    /// <summary>A command constant string value <c>"set-env"</c>.</summary>
    public static readonly string SetEnv = "set-env";
    /// <summary>A command constant string value <c>"add-mask"</c>.</summary>
    public static readonly string AddMask = "add-mask";
    /// <summary>A command constant string value <c>"add-path"</c>.</summary>
    public static readonly string AddPath = "add-path";
    /// <summary>A command constant string value <c>"echo"</c>.</summary>
    public static readonly string Echo = "echo";
    /// <summary>A command constant string value <c>"debug"</c>.</summary>
    public static readonly string Debug = "debug";
    /// <summary>A command constant string value <c>"error"</c>.</summary>
    public static readonly string Error = "error";
    /// <summary>A command constant string value <c>"warning"</c>.</summary>
    public static readonly string Warning = "warning";
    /// <summary>A command constant string value <c>"notice"</c>.</summary>
    public static readonly string Notice = "notice";
    /// <summary>A command constant string value <c>"group"</c>.</summary>
    public static readonly string Group = "group";
    /// <summary>A command constant string value <c>"endgroup"</c>.</summary>
    public static readonly string EndGroup = "endgroup";

    // Deprecated
    // https://github.blog/changelog/2022-10-11-github-actions-deprecating-save-state-and-set-output-commands
    public static readonly string SaveState = "save-state";
    public static readonly string SetOutput = "set-output";

    private static readonly Lazy<string[]> s_all = new(() =>
    [
        SetEnv,
        AddMask,
        AddPath,
        Echo,
        Debug,
        Error,
        Warning,
        Notice,
        Group,
        EndGroup,
        SaveState,
        SetOutput
    ]);

    internal static bool IsConventional(string? command)
    {
        return s_all.Value.Contains(command);
    }
}
