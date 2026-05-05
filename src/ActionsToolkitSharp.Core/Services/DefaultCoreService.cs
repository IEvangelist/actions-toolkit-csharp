// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Services;

/// <inheritdoc cref="ICoreService" />
internal sealed class DefaultCoreService(
    IConsole console,
    ICommandIssuer commandIssuer,
    IFileCommandIssuer fileCommandIssuer) : ICoreService
{
    /// <inheritdoc />
    public Summary Summary { get; } = new();

    /// <inheritdoc />
    public bool IsDebug => GetEnvironmentVariable(RUNNER_DEBUG) is "1";

    /// <inheritdoc />
    public async ValueTask AddPathAsync(string path)
    {
        var filePath = GetEnvironmentVariable(GITHUB_PATH);
        if (filePath is not null)
        {
            await fileCommandIssuer.IssueFileCommandAsync(PATH, path);
        }
        else
        {
            commandIssuer.IssueCommand(CommandNames.AddPath, null, path);
        }

        SetEnvironmentVariable(
            PATH,
            $"{path}{PathSeparator}{GetEnvironmentVariable(PATH)}");
    }

    /// <inheritdoc />
    public void WriteDebug(string message)
    {
        commandIssuer.IssueCommand(
            CommandNames.Debug, message: message);
    }

    /// <inheritdoc />
    public void EndGroup()
    {
        commandIssuer.Issue(
            CommandNames.EndGroup, "");
    }

    /// <inheritdoc />
    public void WriteError(string message, AnnotationProperties? properties = default)
    {
        commandIssuer.IssueCommand(
            CommandNames.Error, properties?.ToCommandProperties(), message);
    }

    /// <inheritdoc />
    public async ValueTask ExportVariableAsync(string name, string value)
    {
        value = value.ToCommandValue();
        SetEnvironmentVariable(name, value);

        var filePath = GetEnvironmentVariable(GITHUB_ENV);
        if (filePath is not null)
        {
            await fileCommandIssuer.IssueFileCommandAsync(
                ENV,
                fileCommandIssuer.PrepareKeyValueMessage(name, value));
        }
        else
        {
            commandIssuer.IssueCommand(
                CommandNames.SetEnv,
                name.ToCommandProperties());
        }
    }

    /// <inheritdoc />
    public bool GetBoolInput(string name, InputOptions? options = default)
    {
        var value = GetInput(name, options);

        return bool.TryParse(value, out var result)
            ? result
            : throw new InvalidOperationException($"""
                Input does not meet YAML 1.2 "Core Schema" specification: {name}
                Support boolean input list: \`true | True | TRUE | false | False | FALSE\`
                """);
    }

    /// <inheritdoc />
    public string GetInput(string name, InputOptions? options = default)
    {
        var value = GetEnvironmentVariable(
            $"{INPUT_}{name.Replace(' ', '_').ToUpperInvariant()}");

        return options.HasValue && options.Value is { Required: true } &&
            string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException(
                $"Input required and not supplied: {name}")
            : options.HasValue && options.Value is { TrimWhitespace: false }
                ? value ?? ""
                : value?.Trim() ?? "";
    }

    /// <inheritdoc />
    public string[] GetMultilineInput(string name, InputOptions? options = default)
    {
        var inputs = GetInput(name, options)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        return options.HasValue && options.Value is { TrimWhitespace: false }
            ? inputs
            : inputs.Select(input => input.Trim()).ToArray();
    }

    /// <inheritdoc />
    public string GetState(string name)
    {
        return GetEnvironmentVariable($"{STATE_}{name}") ?? "";
    }

    /// <inheritdoc />
    public async ValueTask<T> GroupAsync<T>(string name, Func<ValueTask<T>> task)
    {
        T result;
        try
        {
            StartGroup(name);

            result = await task();
        }
        finally
        {
            EndGroup();
        }

        return result;
    }

    /// <inheritdoc />
    public void WriteInfo(string message)
    {
        console.WriteLine(message);
    }

    /// <inheritdoc />
    public void WriteNotice(string message, AnnotationProperties? properties = default)
    {
        commandIssuer.IssueCommand(
            CommandNames.Notice, properties?.ToCommandProperties(), message);
    }

    /// <inheritdoc />
    public async ValueTask SaveStateAsync<T>(string name, T value, JsonTypeInfo<T>? typeInfo)
    {
        var filePath = GetEnvironmentVariable(GITHUB_STATE);
        if (filePath is not null)
        {
            await fileCommandIssuer.IssueFileCommandAsync(
                STATE,
                fileCommandIssuer.PrepareKeyValueMessage(name, value, typeInfo));
        }
        else
        {
            commandIssuer.IssueCommand(
                CommandNames.SaveState,
                name.ToCommandProperties(),
                value.ToCommandValue(typeInfo));
        }
    }

    /// <inheritdoc />
    public void SetCommandEcho(bool enabled)
    {
        commandIssuer.Issue(
            CommandNames.Echo, enabled ? "on" : "off");
    }

    /// <inheritdoc />
    public void SetFailed(string message)
    {
        Environment.ExitCode = (int)ExitCode.Failure;
        WriteError(message);
    }

    /// <inheritdoc />
    public async ValueTask SetOutputAsync<T>(string name, T value, JsonTypeInfo<T>? typeInfo)
    {
        var filePath = GetEnvironmentVariable(GITHUB_OUTPUT);
        if (filePath is not null)
        {
            await fileCommandIssuer.IssueFileCommandAsync(
                OUTPUT,
                fileCommandIssuer.PrepareKeyValueMessage(name, value, typeInfo));
        }
        else
        {
            console.WriteLine("");

            commandIssuer.IssueCommand(
                CommandNames.SetOutput,
                name.ToCommandProperties(),
                value.ToCommandValue(typeInfo));
        }
    }

    /// <inheritdoc />
    public void SetSecret(string secret)
    {
        commandIssuer.IssueCommand(
            CommandNames.AddMask, null, secret);
    }

    /// <inheritdoc />
    public void StartGroup(string name)
    {
        commandIssuer.Issue(
            CommandNames.Group, name);
    }

    /// <inheritdoc />
    public void WriteWarning(string message, AnnotationProperties? properties = default)
    {
        commandIssuer.IssueCommand(
            CommandNames.Warning, properties?.ToCommandProperties(), message);
    }
}
