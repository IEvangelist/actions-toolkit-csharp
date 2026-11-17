// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ActionsToolkit.Scripting.Tests;

internal sealed class EnvironmentScope : IDisposable
{
    private readonly Dictionary<string, string?> _original;

    internal EnvironmentScope(Dictionary<string, string?> values)
    {
        _original = values.ToDictionary(pair => pair.Key, pair => Environment.GetEnvironmentVariable(pair.Key));
        foreach (var (key, value) in values)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    public void Dispose()
    {
        foreach (var (key, value) in _original)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

internal sealed class TestDirectory : IDisposable
{
    internal string Root { get; } = Path.Combine(Directory.GetCurrentDirectory(), $".scripting-tests-{Guid.NewGuid():N}");

    internal TestDirectory() => Directory.CreateDirectory(Root);

    internal string CreateFile(string name, string content = "")
    {
        var path = Path.Combine(Root, name);
        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose() => Directory.Delete(Root, recursive: true);
}

internal sealed record Report(string Name, int Count);

[JsonSerializable(typeof(Report))]
internal sealed partial class TestJsonContext : JsonSerializerContext;
