// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Tests;

/// <summary>
/// Mirrors the upstream <c>__tests__/attest.test.ts</c> suite.
/// </summary>
public class AttestTests
{
    [Fact(DisplayName = "returns an attestation when called with a single subject")]
    public async Task ReturnsAttestationForSingleSubject()
    {
        var signer = new StubAttestSigner();
        var store = new StubAttestationStore(id: "abc-123");
        var sut = new DefaultAttestService(signer, store, new StubProvenancePredicateBuilder());

        var result = await sut.AttestAsync(new AttestOptions
        {
            SubjectName = TestData.SubjectName,
            SubjectDigest = TestData.SubjectDigest,
            PredicateType = TestData.PredicateType,
            Predicate = TestData.PredicateParams,
            Token = "fake",
        }).ConfigureAwait(true);

        Assert.Equal("abc-123", result.AttestationId);
        Assert.Equal(SigstoreInstance.PublicGood, signer.LastInstance);
        Assert.Equal(1, store.CallCount);

        var statement = JsonNode.Parse(signer.LastStatementJson!)!.AsObject();
        Assert.Equal("https://in-toto.io/Statement/v1", statement["_type"]!.GetValue<string>());
        Assert.Equal(TestData.PredicateType, statement["predicateType"]!.GetValue<string>());
        Assert.Single(statement["subject"]!.AsArray());
    }

    [Fact(DisplayName = "returns an attestation when called with multiple subjects")]
    public async Task ReturnsAttestationForMultipleSubjects()
    {
        var signer = new StubAttestSigner();
        var store = new StubAttestationStore();
        var sut = new DefaultAttestService(signer, store, new StubProvenancePredicateBuilder());

        Subject[] subjects =
        [
            new() { Name = "first", Digest = new Dictionary<string, string> { ["sha256"] = "00" } },
            new() { Name = "second", Digest = new Dictionary<string, string> { ["sha256"] = "11" } },
        ];

        var result = await sut.AttestAsync(new AttestOptions
        {
            Subjects = subjects,
            PredicateType = TestData.PredicateType,
            Predicate = TestData.PredicateParams,
            Token = "fake",
        }).ConfigureAwait(true);

        Assert.NotNull(result.Bundle);
        var statement = JsonNode.Parse(signer.LastStatementJson!)!.AsObject();
        Assert.Equal(2, statement["subject"]!.AsArray().Count);
    }

    [Fact(DisplayName = "skips the persistence step when SkipWrite is true")]
    public async Task SkipsPersistenceWhenSkipWrite()
    {
        var store = new StubAttestationStore();
        var sut = new DefaultAttestService(new StubAttestSigner(), store, new StubProvenancePredicateBuilder());

        var result = await sut.AttestAsync(new AttestOptions
        {
            SubjectName = TestData.SubjectName,
            SubjectDigest = TestData.SubjectDigest,
            PredicateType = TestData.PredicateType,
            Predicate = TestData.PredicateParams,
            Token = "fake",
            SkipWrite = true,
        }).ConfigureAwait(true);

        Assert.Null(result.AttestationId);
        Assert.Equal(0, store.CallCount);
    }

    [Fact(DisplayName = "honors a SigstoreInstance.GitHub override")]
    public async Task HonorsSigstoreInstanceOverride()
    {
        var signer = new StubAttestSigner();
        var sut = new DefaultAttestService(signer, new StubAttestationStore(), new StubProvenancePredicateBuilder());

        await sut.AttestAsync(new AttestOptions
        {
            SubjectName = TestData.SubjectName,
            SubjectDigest = TestData.SubjectDigest,
            PredicateType = TestData.PredicateType,
            Predicate = TestData.PredicateParams,
            Token = "fake",
            Sigstore = SigstoreInstance.GitHub,
        }).ConfigureAwait(true);

        Assert.Equal(SigstoreInstance.GitHub, signer.LastInstance);
    }

    [Fact(DisplayName = "throws when neither subject nor subjects is provided")]
    public async Task ThrowsWhenNoSubjectProvided()
    {
        var sut = new DefaultAttestService(new StubAttestSigner(), new StubAttestationStore(), new StubProvenancePredicateBuilder());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.AttestAsync(new AttestOptions
        {
            PredicateType = TestData.PredicateType,
            Predicate = TestData.PredicateParams,
            Token = "fake",
        }).AsTask()).ConfigureAwait(true);

        Assert.Contains(
            "Must provide either subjectName and subjectDigest or subjects",
            ex.Message,
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "throws when token is missing")]
    public async Task ThrowsWhenTokenMissing()
    {
        var sut = new DefaultAttestService(new StubAttestSigner(), new StubAttestationStore(), new StubProvenancePredicateBuilder());

        await Assert.ThrowsAsync<ArgumentException>(() => sut.AttestAsync(new AttestOptions
        {
            SubjectName = TestData.SubjectName,
            SubjectDigest = TestData.SubjectDigest,
            PredicateType = TestData.PredicateType,
            Predicate = TestData.PredicateParams,
            Token = " ",
        }).AsTask()).ConfigureAwait(true);
    }
}
