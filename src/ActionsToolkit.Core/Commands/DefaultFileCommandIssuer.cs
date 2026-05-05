// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Commands;

/// <inheritdoc cref="IFileCommandIssuer" />
internal sealed class DefaultFileCommandIssuer(
    Func<string, string, ValueTask> writeLineTask) : IFileCommandIssuer
{
    private readonly Func<string, string, ValueTask> _writeLineTask = writeLineTask.ThrowIfNull();

    /// <inheritdoc />
    ValueTask IFileCommandIssuer.IssueFileCommandAsync<TValue>(
        string commandSuffix, TValue message, JsonTypeInfo<TValue>? typeInfo)
    {
        var filePath = GetEnvironmentVariable($"{GITHUB_}{commandSuffix}");

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "Unable to find environment variable for file " +
                $"command suffix '{commandSuffix} ({GITHUB_}{commandSuffix})'.");
        }

        return File.Exists(filePath) switch
        {
            false => throw new FileNotFoundException(
                $"Missing file at path: '{filePath}' for file command '{commandSuffix}'."),

            _ => _writeLineTask.Invoke(filePath, message.ToCommandValue(typeInfo))
        };
    }

    /// <inheritdoc />
    string IFileCommandIssuer.PrepareKeyValueMessage<TValue>(
        string key, TValue value, JsonTypeInfo<TValue>? typeInfo)
    {
        var delimiter = $"ghadelimiter_{Guid.NewGuid()}";
        var convertedValue = value.ToCommandValue(typeInfo);

        // These should realistically never happen, but just in case someone finds a
        // way to exploit guid generation let's not allow keys or values that contain
        // the delimiter.
        if (key.Contains(delimiter, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Unexpected input: name should not contain the delimiter {delimiter}");
        }

        if (convertedValue.Contains(delimiter, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Unexpected input: value should not contain the delimiter {delimiter}");
        }

        return $"{key}<<{delimiter}{NewLine}{convertedValue}{NewLine}{delimiter}";
    }
}
