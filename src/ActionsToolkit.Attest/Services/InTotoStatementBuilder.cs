// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Services;

/// <summary>
/// Builds in-toto v1 statements from a subject + predicate pair. Mirrors the
/// <c>buildIntotoStatement</c> function from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src/intoto.ts">
/// <c>actions/toolkit:packages/attest/src/intoto.ts</c></a>.
/// </summary>
internal static class InTotoStatementBuilder
{
    /// <summary>
    /// The in-toto Statement v1 type URI.
    /// </summary>
    internal const string InTotoStatementV1Type = "https://in-toto.io/Statement/v1";

    /// <summary>
    /// Assembles the supplied <paramref name="subjects"/> and
    /// <paramref name="predicate"/> into an
    /// <see cref="InTotoStatement"/> per the in-toto v1 spec.
    /// </summary>
    public static InTotoStatement Build(
        IReadOnlyList<Subject> subjects,
        Predicate predicate)
    {
        ArgumentNullException.ThrowIfNull(subjects);
        ArgumentNullException.ThrowIfNull(predicate);

        return new InTotoStatement
        {
            Type = InTotoStatementV1Type,
            Subject = subjects,
            PredicateType = predicate.Type,
            Predicate = predicate.Params,
        };
    }
}
