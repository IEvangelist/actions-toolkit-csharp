// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal;

/// <summary>
/// JSON source-generated metadata for the small subset of the GitHub public
/// REST API surface that the artifact client uses for cross-workflow
/// (<see cref="FindBy"/>) operations.
/// </summary>
[JsonSerializable(typeof(PublicArtifactsListResponse))]
[JsonSerializable(typeof(PublicArtifactItem))]
[JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.AllowReadingFromString)]
internal sealed partial class PublicArtifactsJsonContext : JsonSerializerContext;
