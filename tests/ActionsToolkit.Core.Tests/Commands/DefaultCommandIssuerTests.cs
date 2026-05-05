// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Tests.Commands;

public sealed class DefaultCommandIssuerTests
{
    [Fact]
    public void IssuesCorrectly()
    {
        var testConsole = new TestConsole();
        var sut = new DefaultCommandIssuer(testConsole);

        sut.Issue(
            commandName: "command",
            message: "message");

        Assert.Equal(
            expected: $"""
                Issuing unconventional command.{Environment.NewLine}::command::message{Environment.NewLine}
                """,
            actual: testConsole.Output.ToString());
    }

    [Fact]
    public void IssuesCommandCorrectly()
    {
        var testConsole = new TestConsole();
        var sut = new DefaultCommandIssuer(testConsole);

        sut.IssueCommand(
            commandName: "command",
            properties: null,
            message: "message");

        Assert.Equal(
            expected: $"""
                Issuing unconventional command.{Environment.NewLine}::command::message{Environment.NewLine}
                """,
            actual: testConsole.Output.ToString());
    }

#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static TheoryData<Dictionary<string, string>?, string, string?> WritesOutputInput =
#pragma warning restore CA2211 // Non-constant fields should not be visible
    new()
    {
        {
            new Dictionary<string, string>
            {
                ["name"] = "summary"
            },
            "Everything worked as expected",
            $"::{CommandNames.SetOutput} name=summary::Everything worked as expected"
        },
        {
            null!,
            "deftones",
            $"::{CommandNames.SetOutput}::deftones"
        },
        {
            new Dictionary<string, string>
            {
                ["name"] = "percent % percent % cr \r cr \r lf \n lf \n colon : colon : comma , comma ,"
            },
            null!,
            $"::{CommandNames.SetOutput} name=percent %25 percent %25 cr %0D cr %0D lf %0A lf %0A colon %3A colon %3A comma %2C comma %2C::"
        },
        {
            null!,
            "%25 %25 %0D %0D %0A %0A %3A %3A %2C %2C",
            $"::{CommandNames.SetOutput}::%2525 %2525 %250D %250D %250A %250A %253A %253A %252C %252C"
        },
        {
            new Dictionary<string, string>
            {
                ["prop1"] = "Value 1",
                ["prop2"] = "Value 2"
            },
            "example",
            $"::{CommandNames.SetOutput} prop1=Value 1,prop2=Value 2::example"
        },
        {
            new Dictionary<string, string>
            {
                ["prop1"] = JsonSerializer.Serialize(new TestObject("object"), TestObjectContext.Default.TestObject),
                ["prop2"] = "123",
                ["prop3"] = "true"
            },
            JsonSerializer.Serialize(new TestObject("object"), TestObjectContext.Default.TestObject).ToCommandValue(),
            $$"""
            ::{{CommandNames.SetOutput}} prop1={"test"%3A"object"},prop2=123,prop3=true::{"test":"object"}
            """
        }
    };

    [Theory]
    [MemberData(nameof(WritesOutputInput))]
    public void IssuesCommandWithPropertiesCorrectly(
        Dictionary<string, string>? properties = null,
        string? message = null,
        string? expected = null)
    {
        var testConsole = new TestConsole();
        var sut = new DefaultCommandIssuer(testConsole);

        sut.IssueCommand(
            commandName: CommandNames.SetOutput,
            properties,
            message);

        Assert.Equal(
            expected: $"""
                {expected}{Environment.NewLine}
                """,
            actual: testConsole.Output.ToString());
    }
}

public record class TestObject([property: JsonPropertyName("test")] string Test);

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(TestObject))]
internal sealed partial class TestObjectContext : JsonSerializerContext
{
}
