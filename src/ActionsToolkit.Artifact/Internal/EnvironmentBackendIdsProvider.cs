// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// Default <see cref="IBackendIdsProvider"/> that reads the
/// <c>ACTIONS_RUNTIME_TOKEN</c> environment variable on first access and
/// memoizes the parsed <see cref="BackendIds"/>.
/// </summary>
internal sealed class EnvironmentBackendIdsProvider : IBackendIdsProvider
{
    private const string RuntimeTokenEnvironmentVariable = "ACTIONS_RUNTIME_TOKEN";

    private readonly Lazy<BackendIds> _ids;

    public EnvironmentBackendIdsProvider()
    {
        _ids = new Lazy<BackendIds>(static () =>
        {
            var token = Environment.GetEnvironmentVariable(RuntimeTokenEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidArtifactTokenException(
                    $"The {RuntimeTokenEnvironmentVariable} environment variable is not set.");
            }

            return ActionsRuntimeJwt.ParseBackendIds(token);
        });
    }

    /// <inheritdoc />
    public BackendIds Get() => _ids.Value;
}
