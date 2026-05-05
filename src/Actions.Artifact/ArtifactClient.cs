namespace Actions.Artifact;

public sealed partial class ArtifactClient : IDisposable
{
    private readonly ArtifactServiceClient _client;
    private readonly BackendIds _backendIds;

    public ArtifactClient(string runtimeToken, Uri resultsServiceUrl)
    {
        ArgumentNullException.ThrowIfNull(runtimeToken);
        ArgumentNullException.ThrowIfNull(resultsServiceUrl);

        _client = new ArtifactServiceClient(runtimeToken, resultsServiceUrl);
        _backendIds = BackendIds.FromToken(runtimeToken);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
