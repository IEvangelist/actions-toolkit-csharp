// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Models;

/// <summary>
/// Identifies which Sigstore signing instance to target. Mirrors the
/// <c>SigstoreInstance</c> string union from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/endpoints.ts">
/// <c>actions/toolkit:packages/attest/src/endpoints.ts</c></a>.
/// </summary>
public enum SigstoreInstance
{
    /// <summary>
    /// The Sigstore "public good" instance (<c>fulcio.sigstore.dev</c> /
    /// <c>rekor.sigstore.dev</c>). Used for attestations on public
    /// repositories.
    /// </summary>
    PublicGood = 0,

    /// <summary>
    /// The GitHub-hosted Sigstore instance (<c>fulcio.githubapp.com</c>,
    /// <c>timestamp.githubapp.com</c>). Used for attestations on private and
    /// internal repositories.
    /// </summary>
    GitHub = 1,
}
