// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.Tests.Workflows;

public sealed class CommandTests
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static TheoryData<string?, string, Dictionary<string, string>?, string?> CommandToStringInput =
#pragma warning restore CA2211 // Non-constant fields should not be visible
    new()
    {
        {
            "some-cmd", "7", null!, "::some-cmd::7"
        },
        {
            "another-name", "true", null!, "::another-name::true"
        },
        {
            "cmdr", "false", new Dictionary<string, string> { ["k1"] = "v1" }, "::cmdr k1=v1::false"
        },
        {
            "~~~", "Hi friends!", null!, "::~~~::Hi friends!"
        },
        {
            null!, null!, null!, "::::"
        }
    };

    [Theory]
    [MemberData(nameof(CommandToStringInput))]
    public void CommandToStringTest(
        string? name = null,
        string message = default!,
        Dictionary<string, string>? properties = null,
        string? expected = null)
    {
        Command command = new(name, message, properties);

        var actual = command.ToString();

        Assert.Equal(expected, actual);
    }
}
