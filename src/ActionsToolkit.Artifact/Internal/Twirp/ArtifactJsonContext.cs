// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Artifact.Internal.Twirp;

[JsonSerializable(typeof(CreateArtifactRequest))]
[JsonSerializable(typeof(CreateArtifactResponse))]
[JsonSerializable(typeof(FinalizeArtifactRequest))]
[JsonSerializable(typeof(FinalizeArtifactResponse))]
[JsonSerializable(typeof(ListArtifactsRequest))]
[JsonSerializable(typeof(ListArtifactsResponse))]
[JsonSerializable(typeof(MonolithArtifact))]
[JsonSerializable(typeof(GetSignedArtifactUrlRequest))]
[JsonSerializable(typeof(GetSignedArtifactUrlResponse))]
[JsonSerializable(typeof(DeleteArtifactRequest))]
[JsonSerializable(typeof(DeleteArtifactResponse))]
[JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.AllowReadingFromString)]
internal sealed partial class ArtifactJsonContext : JsonSerializerContext;
