// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Artifact;

/// <summary>
/// AOT-clean parser for the GitHub Actions <c>ACTIONS_RUNTIME_TOKEN</c> JWT.
/// Extracts the <see cref="BackendIds"/> encoded in the <c>scp</c> claim under
/// the <c>Actions.Results:&lt;run&gt;:&lt;job&gt;</c> scope. Replaces the
/// <c>Microsoft.IdentityModel.JsonWebTokens</c> dependency that PR #4 used,
/// keeping the package free of reflection-based JWT machinery so it stays
/// safe for <c>PublishAot=true</c>.
/// </summary>
internal static class ActionsRuntimeJwt
{
    private const string ResultsScopePrefix = "Actions.Results";

    /// <summary>
    /// Parses the supplied <paramref name="token"/> and returns the
    /// <see cref="BackendIds"/> embedded in the <c>Actions.Results</c> scope
    /// claim.
    /// </summary>
    /// <param name="token">The raw JWT string from
    /// <c>ACTIONS_RUNTIME_TOKEN</c>.</param>
    /// <exception cref="InvalidArtifactTokenException">The token is malformed
    /// or does not contain the expected scope claim.</exception>
    public static BackendIds ParseBackendIds(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var firstDot = token.IndexOf('.');
        if (firstDot <= 0)
        {
            throw new InvalidArtifactTokenException(
                "The ACTIONS_RUNTIME_TOKEN is not a well-formed JWT (missing header separator).");
        }

        var secondDot = token.IndexOf('.', firstDot + 1);
        if (secondDot <= firstDot + 1)
        {
            throw new InvalidArtifactTokenException(
                "The ACTIONS_RUNTIME_TOKEN is not a well-formed JWT (missing payload separator).");
        }

        var payloadSegment = token.AsSpan(firstDot + 1, secondDot - firstDot - 1);

        byte[] payloadBytes;
        try
        {
            payloadBytes = DecodeBase64Url(payloadSegment);
        }
        catch (FormatException ex)
        {
            throw new InvalidArtifactTokenException(
                "The ACTIONS_RUNTIME_TOKEN payload could not be base64url-decoded.", ex);
        }

        string? scope;
        try
        {
            using var document = JsonDocument.Parse(payloadBytes);
            if (!document.RootElement.TryGetProperty("scp", out var scpElement) ||
                scpElement.ValueKind != JsonValueKind.String)
            {
                throw new InvalidArtifactTokenException(
                    "The ACTIONS_RUNTIME_TOKEN payload does not contain a string 'scp' claim.");
            }

            scope = scpElement.GetString();
        }
        catch (JsonException ex)
        {
            throw new InvalidArtifactTokenException(
                "The ACTIONS_RUNTIME_TOKEN payload is not valid JSON.", ex);
        }

        if (string.IsNullOrEmpty(scope))
        {
            throw new InvalidArtifactTokenException(
                "The ACTIONS_RUNTIME_TOKEN 'scp' claim is empty.");
        }

        foreach (var entry in scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = entry.Split(':');
            if (parts.Length == 0 || !string.Equals(parts[0], ResultsScopePrefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (parts.Length != 3 ||
                string.IsNullOrEmpty(parts[1]) ||
                string.IsNullOrEmpty(parts[2]))
            {
                break;
            }

            return new BackendIds(parts[1], parts[2]);
        }

        throw new InvalidArtifactTokenException(
            "The ACTIONS_RUNTIME_TOKEN does not contain a usable 'Actions.Results' scope claim.");
    }

    private static byte[] DecodeBase64Url(ReadOnlySpan<char> input)
    {
        var maxBytes = Base64Url.GetMaxDecodedLength(input.Length);
        var buffer = new byte[maxBytes];
        var status = Base64Url.DecodeFromChars(input, buffer, out _, out var bytesWritten);
        if (status != System.Buffers.OperationStatus.Done)
        {
            throw new FormatException($"Base64Url decode failed with status '{status}'.");
        }

        if (bytesWritten == buffer.Length)
        {
            return buffer;
        }

        var trimmed = new byte[bytesWritten];
        Buffer.BlockCopy(buffer, 0, trimmed, 0, bytesWritten);
        return trimmed;
    }
}
