// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace ActionsToolkit.AotTestSupport;

/// <summary>
/// xUnit collection that ensures the package's <c>AotConsumer</c> binary is
/// published exactly once per test assembly run.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "xUnit collection definitions are conventionally named with a 'Collection' suffix.")]
[CollectionDefinition(Name)]
public sealed class AotConsumerCollection : ICollectionFixture<AotPublishFixture>
{
    public const string Name = "Aot Consumer";
}
