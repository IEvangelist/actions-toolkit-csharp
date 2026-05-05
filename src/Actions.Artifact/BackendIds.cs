using Microsoft.IdentityModel.JsonWebTokens;

namespace Actions.Artifact;

internal sealed class BackendIds
{
    public required string WorkflowRunBackendId { get; init; }
    public required string WorkflowJobRunBackendId { get; init; }

    public static BackendIds FromToken(string token)
    {
        var jsonWebToken = new JsonWebToken(token);

        var scp = jsonWebToken.GetPayloadValue<string>("scp");

        foreach (var scopes in scp.Split(' '))
        {
            var scopeParts = scopes.Split(':');
            if (scopeParts[0] != "Actions.Results")
            {
                continue;
            }

            if (scopeParts.Length != 3)
            {
                break;
            }

            return new BackendIds
            {
                WorkflowRunBackendId = scopeParts[1],
                WorkflowJobRunBackendId = scopeParts[2],
            };
        }

        throw new ArgumentException("Failed to get backend IDs: The provided JWT token is invalid and/or missing claims", nameof(token));
    }
}
