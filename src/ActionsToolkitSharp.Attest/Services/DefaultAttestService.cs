// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Services;

/// <summary>
/// Default <see cref="IAttestService"/> orchestrating the
/// <see cref="InTotoStatementBuilder.Build"/> →
/// <see cref="IAttestSigner.SignAsync"/> →
/// <see cref="IAttestationStore.WriteAsync"/> pipeline. Mirrors the
/// <c>attest</c> and <c>attestProvenance</c> functions from
/// <a href="https://github.com/actions/toolkit/blob/main/packages/attest/src">
/// <c>actions/toolkit:packages/attest/src</c></a>.
/// </summary>
internal sealed class DefaultAttestService : IAttestService
{
    private readonly IAttestSigner _signer;
    private readonly IAttestationStore _store;
    private readonly IProvenancePredicateBuilder _provenanceBuilder;

    public DefaultAttestService(
        IAttestSigner signer,
        IAttestationStore store,
        IProvenancePredicateBuilder provenanceBuilder)
    {
        ArgumentNullException.ThrowIfNull(signer);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(provenanceBuilder);

        _signer = signer;
        _store = store;
        _provenanceBuilder = provenanceBuilder;
    }

    public async ValueTask<Attestation> AttestAsync(
        AttestOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Token);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.PredicateType);
        ArgumentNullException.ThrowIfNull(options.Predicate);

        var subjects = ResolveSubjects(options.Subjects, options.SubjectName, options.SubjectDigest);

        var predicate = new Predicate
        {
            Type = options.PredicateType,
            Params = options.Predicate,
        };

        var statement = InTotoStatementBuilder.Build(subjects, predicate);
        var statementJson = JsonSerializer.Serialize(statement, AttestJsonContext.Default.InTotoStatement);

        var signResult = await _signer
            .SignAsync(statementJson, options.Sigstore ?? SigstoreInstance.PublicGood, cancellationToken)
            .ConfigureAwait(false);

        string? attestationId = null;
        if (!options.SkipWrite)
        {
            attestationId = await _store
                .WriteAsync(signResult.Bundle, options.Token, options.Headers, cancellationToken)
                .ConfigureAwait(false);
        }

        return new Attestation
        {
            Bundle = signResult.Bundle,
            Certificate = signResult.Certificate,
            TlogId = signResult.TlogId,
            AttestationId = attestationId,
        };
    }

    public async ValueTask<Attestation> AttestProvenanceAsync(
        AttestProvenanceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var predicate = await _provenanceBuilder
            .BuildAsync(options.Issuer, cancellationToken)
            .ConfigureAwait(false);

        return await AttestAsync(
            new AttestOptions
            {
                SubjectName = options.SubjectName,
                SubjectDigest = options.SubjectDigest,
                Subjects = options.Subjects,
                Token = options.Token,
                Sigstore = options.Sigstore,
                Headers = options.Headers,
                SkipWrite = options.SkipWrite,
                PredicateType = predicate.Type,
                Predicate = predicate.Params,
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the list of subjects for a sign call from the upstream-style
    /// <c>subjects</c> + (deprecated) <c>subjectName</c>/<c>subjectDigest</c>
    /// triplet. Mirrors the behavior of <c>attest.ts</c> upstream.
    /// </summary>
    internal static IReadOnlyList<Subject> ResolveSubjects(
        IReadOnlyList<Subject>? subjects,
        string? subjectName,
        IReadOnlyDictionary<string, string>? subjectDigest)
    {
        if (subjects is { Count: > 0 })
        {
            return subjects;
        }

        if (!string.IsNullOrWhiteSpace(subjectName) && subjectDigest is { Count: > 0 })
        {
            return new[]
            {
                new Subject { Name = subjectName, Digest = subjectDigest },
            };
        }

        throw new ArgumentException(
            "Must provide either subjectName and subjectDigest or subjects",
            nameof(subjects));
    }
}
