// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Services;

/// <summary>
/// The workflow core service, used to perform various operations in the context of a GitHub Action workflow.
/// Inspired by <a href="https://github.com/actions/toolkit/blob/main/packages/core/src/core.ts"></a>.
/// Specified in <a href="https://docs.github.com/actions/using-workflows/workflow-commands-for-github-actions"></a>
/// </summary>
public interface ICoreService
{
    /// <summary>
    /// Gets the <see cref="Summary"/> used to summarize the workflow.
    /// </summary>
    Summary Summary { get; }

    /// <summary>
    /// Sets env variable for this action and future actions in the job.
    /// </summary>
    /// <param name="name">The name of the variable to se</param>
    /// <param name="value">
    /// The value of the variable. Non-string values will be converted
    /// to a string via <see cref="JsonSerializer.Serialize(object?, Type, JsonSerializerOptions?)"/>
    /// </param>
    ValueTask ExportVariableAsync(string name, string value);

    /// <summary>
    /// Registers a secret which will get masked from logs.
    /// </summary>
    /// <param name="secret">Value of the secret.</param>
    void SetSecret(string secret);

    /// <summary>
    /// Prepends <paramref name="inputPath"/> to the <c>PATH</c> (for this action and future actions).
    /// </summary>
    /// <param name="inputPath">The input path to prepend.</param>
    ValueTask AddPathAsync(string inputPath);

    /// <summary>
    /// Gets the value of an input.
    /// When <see cref="InputOptions.TrimWhitespace"/> is <c>true</c>, the value is also trimmed.
    /// </summary>
    /// <param name="name">name of the input to get.</param>
    /// <param name="options">optional. <see cref="InputOptions"/>.</param>
    /// <returns>Returns an empty string if the value is not defined.</returns>
    string GetInput(string name, InputOptions? options = null);

    /// <summary>
    /// Gets the values of an multiline input.  Each value is also trimmed.
    /// </summary>
    /// <param name="name">name of the input to get</param>
    /// <param name="options">optional. <see cref="InputOptions"/>.</param>
    /// <returns>An array of values for the corresponding input.</returns>
    string[] GetMultilineInput(string name, InputOptions? options = null);

    /// <summary>
    ///  Gets the input value of the bool type in the YAML 1.2 "core schema" specification.
    ///  Support bool input list: <c>true | True | TRUE | false | False | FALSE</c>.
    ///  <a href="https://yaml.org/spec/1.2/spec.html#id2804923"></a>
    /// </summary>
    /// <param name="name">name of the input to get.</param>
    /// <param name="options">optional. <see cref="InputOptions"/>.</param>
    /// <returns>The return value is also in bool type.</returns>
    bool GetBoolInput(string name, InputOptions? options = null);

    /// <summary>
    /// Sets the value of an output.
    /// </summary>
    /// <param name="name">name of the output to set.</param>
    /// <param name="value">value to store.
    /// Non-string values will be converted to a string via <see cref="JsonSerializer.Serialize(object?, JsonTypeInfo)"/>
    /// </param>
    /// <param name="typeInfo">The JSON type info used to serialize.</param>
    ValueTask SetOutputAsync<T>(string name, T value, JsonTypeInfo<T>? typeInfo = null);

    /// <summary>
    /// Enables or disables the echoing of commands into stdout for the rest of the step.
    /// Echoing is disabled by default if <c>ACTIONS_STEP_DEBUG</c> is not set.
    /// </summary>
    /// <param name="enabled">Toggle used to conditionally turn <c>"on"</c> or <c>"off"</c>.</param>
    void SetCommandEcho(bool enabled);

    /// <summary>
    /// Sets the action status to failed.
    /// When the action exits it will be with an exit code of <see cref="ExitCode.Failure"/>.
    /// </summary>
    /// <param name="message">The error issue message.</param>
    void SetFailed(string message);

    /// <summary>
    /// Gets whether the GitHub Action has enabled "Runner Debug", i.e.; <c>RUNNER_DEBUG</c> is set to <c>1</c>.
    /// </summary>
    bool IsDebug { get; }

    /// <summary>
    /// Writes debug message to user log.
    /// </summary>
    /// <param name="message">debug message</param>
    void WriteDebug(string message);

    /// <summary>
    /// Adds an error issue.
    /// </summary>
    /// <param name="message">WriteError issue message.</param>
    /// <param name="properties">Optional properties to add to the annotation.</param>
    void WriteError(string message, AnnotationProperties? properties = null);

    /// <summary>
    /// Adds a warning issue.
    /// </summary>
    /// <param name="message">WriteError issue message.</param>
    /// <param name="properties">Optional properties to add to the annotation.</param>
    void WriteWarning(string message, AnnotationProperties? properties = null);

    /// <summary>
    /// Adds a notice issue.
    /// </summary>
    /// <param name="message">WriteError issue message.</param>
    /// <param name="properties">Optional properties to add to the annotation.</param>
    void WriteNotice(string message, AnnotationProperties? properties = null);

    /// <summary>
    /// Writes info to log with console.log.
    /// </summary>
    /// <param name="message">info message</param>
    void WriteInfo(string message);

    /// <summary>
    /// Begin an output group.
    /// Output until the next <c>end-group</c> will be foldable in this group.
    /// </summary>
    /// <param name="name">The name of the output group.</param>
    void StartGroup(string name);

    /// <summary>
    /// End the output group.
    /// </summary>
    void EndGroup();

    /// <summary>
    /// Wrap an asynchronous function call in a group.
    /// </summary>
    /// <typeparam name="T">
    /// The generic-type parameter used as a result from the given <paramref name="action"/>.
    /// </typeparam>
    /// <param name="name">The name of the group.</param>
    /// <param name="action">The function to wrap in the group.</param>
    ValueTask<T> GroupAsync<T>(string name, Func<ValueTask<T>> action);

    /// <summary>
    /// Saves state for current action, the state can only be retrieved by this action's post job execution
    /// </summary>
    /// <param name="name">name of the state to store</param>
    /// <param name="value">value to store.
    /// Non-string values will be converted to a string via <see cref="JsonSerializer.Serialize(object?, JsonTypeInfo)"/>
    /// </param>
    /// <param name="typeInfo">The JSON type info used to serialize.</param>
    ValueTask SaveStateAsync<T>(string name, T value, JsonTypeInfo<T>? typeInfo = null);

    /// <summary>
    /// Gets the vale of a state set by this action's main execution.
    /// </summary>
    /// <param name="name">name of the state to get</param>
    /// <returns>The string representation of the state.</returns>
    string GetState(string name);
}
