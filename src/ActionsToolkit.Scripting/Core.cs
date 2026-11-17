// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using ActionsToolkit.Core;
using ActionsToolkit.Core.Extensions;
using ActionsToolkit.Core.Services;
using ActionsToolkit.Core.Summaries;
using ActionsToolkit.Core.Workflows;

namespace ActionsToolkit.Scripting;

/// <summary>Runner inputs, commands, outputs, and the step summary.</summary>
public static class Core
{
    private static readonly ICoreService Service =
        ServiceBridge.CreateHelper<ICoreService>(services => services.AddGitHubActionsCore());

    /// <summary>The process-local summary buffer. Mutate from one writer and explicitly call WriteAsync.</summary>
    public static Summary Summary => Service.Summary;

    /// <inheritdoc cref="ICoreService.IsDebug" />
    public static bool IsDebug => Service.IsDebug;

    /// <summary>Reads an input, optionally requiring a nonempty value.</summary>
    public static string GetInput(string name, bool required = false) =>
        Service.GetInput(name, new InputOptions(Required: required));

    /// <summary>Reads a Boolean input, optionally requiring a nonempty value.</summary>
    public static bool GetBoolInput(string name, bool required = false) =>
        Service.GetBoolInput(name, new InputOptions(Required: required));

    /// <summary>Reads a multiline input, optionally requiring a nonempty value.</summary>
    public static string[] GetMultilineInput(string name, bool required = false) =>
        Service.GetMultilineInput(name, new InputOptions(Required: required));

    /// <inheritdoc cref="ICoreService.WriteInfo" />
    public static void Info(string message) => Service.WriteInfo(message);

    /// <inheritdoc cref="ICoreService.WriteDebug" />
    public static void Debug(string message) => Service.WriteDebug(message);

    /// <inheritdoc cref="ICoreService.WriteWarning" />
    public static void Warning(string message, AnnotationProperties? properties = null) =>
        Service.WriteWarning(message, properties);

    /// <inheritdoc cref="ICoreService.WriteError" />
    public static void Error(string message, AnnotationProperties? properties = null) =>
        Service.WriteError(message, properties);

    /// <inheritdoc cref="ICoreService.WriteNotice" />
    public static void Notice(string message, AnnotationProperties? properties = null) =>
        Service.WriteNotice(message, properties);

    /// <summary>Writes a string step output without JSON quoting.</summary>
    public static ValueTask SetOutputAsync(string name, string value) =>
        Service.SetOutputAsync(name, value);

    /// <summary>Writes a lowercase Boolean step output.</summary>
    public static ValueTask SetOutputAsync(string name, bool value) =>
        Service.SetOutputAsync(name, value ? "true" : "false");

    /// <summary>Writes a numeric step output using invariant culture, without JSON metadata.</summary>
    public static ValueTask SetOutputAsync<T>(string name, T value) where T : INumber<T> =>
        Service.SetOutputAsync(name, value.ToString(null, CultureInfo.InvariantCulture));

    /// <summary>Writes a structured step output using explicit AOT-compatible JSON metadata.</summary>
    public static ValueTask SetOutputAsync<T>(string name, T value, JsonTypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        return Service.SetOutputAsync(name, JsonSerializer.Serialize(value, typeInfo));
    }

    /// <inheritdoc cref="ICoreService.ExportVariableAsync" />
    public static ValueTask ExportVariableAsync(string name, string value) =>
        Service.ExportVariableAsync(name, value);

    /// <inheritdoc cref="ICoreService.AddPathAsync" />
    public static ValueTask AddPathAsync(string inputPath) => Service.AddPathAsync(inputPath);

    /// <inheritdoc cref="ICoreService.SetSecret" />
    public static void SetSecret(string secret) => Service.SetSecret(secret);

    /// <inheritdoc cref="ICoreService.SetFailed" />
    public static void SetFailed(string message) => Service.SetFailed(message);

    /// <inheritdoc cref="ICoreService.SetCommandEcho" />
    public static void SetCommandEcho(bool enabled) => Service.SetCommandEcho(enabled);

    /// <inheritdoc cref="ICoreService.StartGroup" />
    public static void StartGroup(string name) => Service.StartGroup(name);

    /// <inheritdoc cref="ICoreService.EndGroup" />
    public static void EndGroup() => Service.EndGroup();

    /// <inheritdoc cref="ICoreService.GroupAsync{T}" />
    public static ValueTask<T> GroupAsync<T>(string name, Func<ValueTask<T>> action) =>
        Service.GroupAsync(name, action);

    /// <summary>Runs an asynchronous action in a log group, closing it even when the action fails.</summary>
    public static async ValueTask GroupAsync(string name, Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        StartGroup(name);
        try
        {
            await action().ConfigureAwait(false);
        }
        finally
        {
            EndGroup();
        }
    }

    /// <summary>Writes string state for a later phase of the same action, not a separate step.</summary>
    public static ValueTask SaveStateAsync(string name, string value) => Service.SaveStateAsync(name, value);

    /// <summary>Writes JSON state using explicit serialization metadata.</summary>
    public static ValueTask SaveStateAsync<T>(string name, T value, JsonTypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        return Service.SaveStateAsync(name, JsonSerializer.Serialize(value, typeInfo));
    }

    /// <inheritdoc cref="ICoreService.GetState" />
    public static string GetState(string name) => Service.GetState(name);
}
