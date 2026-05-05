// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace ActionsToolkitSharp.Exec;

/// <summary>
/// Cross-platform helpers for parsing a command-line string into argument tokens
/// and applying Windows-specific quoting rules. C# port of the corresponding
/// helpers from <c>actions/toolkit/packages/exec/src/toolrunner.ts</c>.
/// </summary>
[SuppressMessage("Performance", "CA1812", Justification = "Public-by-design helpers used internally and by tests.")]
internal static class ArgString
{
    /// <summary>
    /// Splits an arbitrary command-line string into individual argument tokens.
    /// Honors double-quoted spans and the <c>\"</c> escape sequence inside quotes.
    /// Mirrors upstream <c>argStringToArray</c>.
    /// </summary>
    public static string[] ToArray(string argString)
    {
        ArgumentNullException.ThrowIfNull(argString);

        var args = new List<string>();

        var inQuotes = false;
        var escaped = false;
        var arg = new StringBuilder();

        void Append(char c)
        {
            // we only escape double quotes
            if (escaped && c != '"')
            {
                arg.Append('\\');
            }

            arg.Append(c);
            escaped = false;
        }

        for (var i = 0; i < argString.Length; i++)
        {
            var c = argString[i];

            if (c == '"')
            {
                if (!escaped)
                {
                    inQuotes = !inQuotes;
                }
                else
                {
                    Append(c);
                }
                continue;
            }

            if (c == '\\' && escaped)
            {
                Append(c);
                continue;
            }

            if (c == '\\' && inQuotes)
            {
                escaped = true;
                continue;
            }

            if (c == ' ' && !inQuotes)
            {
                if (arg.Length > 0)
                {
                    args.Add(arg.ToString());
                    arg.Clear();
                }
                continue;
            }

            Append(c);
        }

        if (arg.Length > 0)
        {
            args.Add(arg.ToString().Trim());
        }

        return [.. args];
    }
}
