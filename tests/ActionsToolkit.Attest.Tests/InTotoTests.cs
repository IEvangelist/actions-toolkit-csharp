// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Tests;

/// <summary>
/// Mirrors the upstream <c>__tests__/intoto.test.ts</c> suite.
/// </summary>
public class InTotoTests
{
    [Fact(DisplayName = "returns an in-toto v1 statement")]
    public void ReturnsInTotoStatement()
    {
        Subject[] subjects = [TestData.Subject];
        var statement = InTotoStatementBuilder.Build(subjects, TestData.Predicate);

        Assert.Equal("https://in-toto.io/Statement/v1", statement.Type);
        Assert.Equal(TestData.PredicateType, statement.PredicateType);
        Assert.Equal(subjects, statement.Subject);
        Assert.NotNull(statement.Predicate);

        var json = JsonSerializer.Serialize(statement, AttestJsonContext.Default.InTotoStatement);
        var parsed = JsonNode.Parse(json)!.AsObject();

        Assert.Equal("https://in-toto.io/Statement/v1", parsed["_type"]!.GetValue<string>());
        Assert.Equal(TestData.PredicateType, parsed["predicateType"]!.GetValue<string>());

        var subjectArray = parsed["subject"]!.AsArray();
        Assert.Single(subjectArray);
        Assert.Equal(TestData.SubjectName, subjectArray[0]!["name"]!.GetValue<string>());
        Assert.Equal(TestData.SubjectDigestSha, subjectArray[0]!["digest"]!["sha256"]!.GetValue<string>());

        Assert.Equal("value", parsed["predicate"]!["key"]!.GetValue<string>());
    }

    [Fact(DisplayName = "throws when subjects collection is null")]
    public void ThrowsOnNullSubjects() =>
        Assert.Throws<ArgumentNullException>(() =>
            InTotoStatementBuilder.Build(null!, TestData.Predicate));

    [Fact(DisplayName = "throws when predicate is null")]
    public void ThrowsOnNullPredicate() =>
        Assert.Throws<ArgumentNullException>(() =>
            InTotoStatementBuilder.Build([TestData.Subject], null!));
}
