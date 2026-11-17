// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace ActionsToolkit.Runner;

internal sealed record LauncherOptions(
    string Workspace,
    string Script,
    IReadOnlyList<string> Arguments,
    IReadOnlyDictionary<string, string> Inputs,
    string? Token)
{
    internal const string ScriptVariable = "ACTIONS_TOOLKIT_SCRIPT";
    internal const string ArgumentsVariable = "ACTIONS_TOOLKIT_ARGS";
    internal const string InputsVariable = "ACTIONS_TOOLKIT_INPUTS";
    internal const string TokenVariable = "ACTIONS_TOOLKIT_GITHUB_TOKEN";

    internal static readonly string[] ControlVariables =
    [
        ScriptVariable, ArgumentsVariable, InputsVariable, TokenVariable,
        "INPUT_SCRIPT", "INPUT_ARGS", "INPUT_INPUTS", "INPUT_GITHUB-TOKEN", "INPUT_GITHUB_TOKEN"
    ];

    internal static LauncherOptions Parse(IReadOnlyDictionary<string, string?> environment)
    {
        var workspace = Get(environment, "GITHUB_WORKSPACE");
        var script = Get(environment, ScriptVariable);
        if (string.IsNullOrWhiteSpace(workspace) || !Directory.Exists(workspace))
        {
            throw new ArgumentException("GITHUB_WORKSPACE must name an existing checked-out directory.");
        }

        if (string.IsNullOrWhiteSpace(script) || script.Contains('\0') ||
            script.Contains('\\') || Path.IsPathRooted(script) ||
            !script.EndsWith(".cs", StringComparison.Ordinal))
        {
            throw new ArgumentException("script must be a workspace-relative .cs file path using forward slashes.");
        }

        var realWorkspace = ResolveExistingPath(Path.GetFullPath(workspace));
        var candidate = Path.GetFullPath(script, realWorkspace);
        RequireContained(realWorkspace, candidate);
        var realScript = ResolveExistingPath(candidate, realWorkspace);
        if (!File.Exists(realScript))
        {
            throw new ArgumentException("script must name an existing .cs file inside GITHUB_WORKSPACE.");
        }

        var arguments = ParseArguments(Get(environment, ArgumentsVariable));
        var inputs = ParseInputs(Get(environment, InputsVariable));
        var token = Get(environment, TokenVariable);
        if (token?.Contains('\0') == true)
        {
            throw new ArgumentException("github-token cannot contain NUL.");
        }

        return new(realWorkspace, realScript, arguments, inputs, token);
    }

    private static string? Get(IReadOnlyDictionary<string, string?> environment, string key) =>
        environment.TryGetValue(key, out var value) ? value : null;

    private static List<string> ParseArguments(string? json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrEmpty(json) ? "[]" : json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException("args must be a JSON array of strings.");
        }

        var result = new List<string>();
        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String || item.GetString()!.Contains('\0'))
            {
                throw new ArgumentException("args must contain only strings without NUL.");
            }

            result.Add(item.GetString()!);
        }

        return result;
    }

    private static Dictionary<string, string> ParseInputs(string? json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrEmpty(json) ? "{}" : json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("inputs must be a JSON object of string values.");
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            var name = property.Name;
            if (name.Length == 0 || !char.IsAsciiLetterOrDigit(name[0]) ||
                name.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ' ' and not '_' and not '-'))
            {
                throw new ArgumentException("Input names must start with an ASCII letter or digit and contain only letters, digits, spaces, underscores or hyphens.");
            }

            var normalized = name.Replace(' ', '_').ToUpperInvariant();
            var variable = $"INPUT_{normalized}";
            if (ControlVariables.Contains(variable, StringComparer.OrdinalIgnoreCase) ||
                normalized.StartsWith("GITHUB_", StringComparison.Ordinal) ||
                normalized.StartsWith("ACTIONS_", StringComparison.Ordinal) ||
                normalized.StartsWith("RUNNER_", StringComparison.Ordinal))
            {
                throw new ArgumentException("inputs cannot contain launcher control or runner-reserved names.");
            }

            if (property.Value.ValueKind != JsonValueKind.String || property.Value.GetString()!.Contains('\0'))
            {
                throw new ArgumentException("inputs must contain only string values without NUL.");
            }

            if (!result.TryAdd(variable, property.Value.GetString()!))
            {
                throw new ArgumentException("inputs contains duplicate names after INPUT_* normalization.");
            }
        }

        return result;
    }

    private static void RequireContained(string root, string path)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var prefix = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, comparison))
        {
            throw new ArgumentException("script must remain inside the real GITHUB_WORKSPACE, including symbolic links.");
        }
    }

    private static string ResolveExistingPath(string path, string? boundary = null, int links = 0)
    {
        if (links > 64)
        {
            throw new ArgumentException("script contains too many symbolic links.");
        }

        var current = boundary ?? Path.GetPathRoot(path)!;
        var relative = boundary is null ? path[current.Length..] : Path.GetRelativePath(boundary, path);
        foreach (var part in relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, part);
            var attributes = File.GetAttributes(current);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                FileSystemInfo entry = (attributes & FileAttributes.Directory) != 0
                    ? new DirectoryInfo(current)
                    : new FileInfo(current);
                var target = entry.ResolveLinkTarget(returnFinalTarget: false)?.FullName
                    ?? throw new ArgumentException("script contains an unresolved symbolic link.");
                // Link targets can themselves contain symlinked parent directories.
                current = ResolveExistingPath(target, links: links + 1);
            }

            if (boundary is not null)
            {
                RequireContained(boundary, current);
            }
        }

        return current;
    }
}
