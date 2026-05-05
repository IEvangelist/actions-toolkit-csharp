// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkitSharp.HttpClient;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal sealed class DefaultHttpKeyedClientFactory(
    IHttpClientFactory clientFactory) : IHttpCredentialClientFactory
{
    IHttpClient IHttpCredentialClientFactory.CreateClient()
    {
        var client = clientFactory.CreateClient(ClientNames.Basic);

        return new DefaultHttpClient(client);
    }

    IHttpClient IHttpCredentialClientFactory.CreateBasicClient(string username, string password)
    {
        var client = clientFactory.CreateClient(ClientNames.Basic);
        var requestHandler = new BasicCredentialHandler(username, password);

        return new DefaultHttpClient(client, requestHandler);
    }

    IHttpClient IHttpCredentialClientFactory.CreateBearerTokenClient(string token)
    {
        var client = clientFactory.CreateClient(ClientNames.Bearer);
        var requestHandler = new BearerCredentialHandler(token);

        return new DefaultHttpClient(client, requestHandler);
    }

    IHttpClient IHttpCredentialClientFactory.CreatePersonalAccessTokenClient(string pat)
    {
        var client = clientFactory.CreateClient(ClientNames.Pat);
        var requestHandler = new PersonalAccessTokenHandler(pat);

        return new DefaultHttpClient(client, requestHandler);
    }
}
