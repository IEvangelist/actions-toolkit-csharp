// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact.Tests;

/// <summary>
/// Restores the value of an environment variable on dispose, so tests can
/// safely mutate process-level configuration.
/// </summary>
internal sealed class EnvironmentScope : IDisposable
{
    private readonly string _key;
    private readonly string? _previous;

    public EnvironmentScope(string key, string? value)
    {
        _key = key;
        _previous = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
    }

    public void Dispose() =>
        Environment.SetEnvironmentVariable(_key, _previous);
}
