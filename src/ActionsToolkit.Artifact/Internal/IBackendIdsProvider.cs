// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// Resolves the <see cref="BackendIds"/> required by every Twirp RPC against
/// the GitHub Actions results service. The default implementation reads the
/// <c>ACTIONS_RUNTIME_TOKEN</c> environment variable and parses the encoded
/// JWT scope claim; tests may substitute a fake.
/// </summary>
internal interface IBackendIdsProvider
{
    /// <summary>
    /// Returns the resolved <see cref="BackendIds"/>.
    /// </summary>
    /// <exception cref="InvalidArtifactTokenException">The runtime token is
    /// missing, malformed, or does not contain a usable
    /// <c>Actions.Results</c> scope claim.</exception>
    BackendIds Get();
}
