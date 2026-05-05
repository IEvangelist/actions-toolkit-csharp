// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Attest.Json;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for every DTO that
/// crosses an HTTP boundary in <c>ActionsToolkitSharp.Attest</c>. Required for
/// Native AOT (no reflection-based serialization) per the package's
/// <c>&lt;IsAotCompatible&gt;true&lt;/IsAotCompatible&gt;</c> setting.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
[JsonSerializable(typeof(InTotoStatement))]
[JsonSerializable(typeof(Subject))]
[JsonSerializable(typeof(Predicate))]
[JsonSerializable(typeof(Attestation))]
[JsonSerializable(typeof(AttestOptions))]
[JsonSerializable(typeof(AttestProvenanceOptions))]
[JsonSerializable(typeof(SigstoreEndpoints))]
[JsonSerializable(typeof(OidcTokenResponse))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(IReadOnlyDictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(string))]
internal sealed partial class AttestJsonContext : JsonSerializerContext
{
}
