// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Cache.Tests;

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

/// <summary>
/// Convenience for atomically swapping a set of environment variables for
/// the lifetime of a test.
/// </summary>
internal sealed class EnvironmentScopeBag : IDisposable
{
    private readonly List<EnvironmentScope> _scopes = [];

    public EnvironmentScopeBag Set(string key, string? value)
    {
        _scopes.Add(new EnvironmentScope(key, value));
        return this;
    }

    public void Dispose()
    {
        for (var i = _scopes.Count - 1; i >= 0; i--)
        {
            _scopes[i].Dispose();
        }
    }
}
