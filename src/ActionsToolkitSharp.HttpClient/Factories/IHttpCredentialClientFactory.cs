// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ActionsToolkitSharp.HttpClient;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represents a factory for creating <see cref="IHttpClient"/> instances, given a set of credentials.
/// </summary>
public interface IHttpCredentialClientFactory
{
    /// <summary>
    /// Creates a new <see cref="IHttpClient"/> instance, without any credentials.
    /// </summary>
    /// <returns>A new <see cref="IHttpClient"/> instance.</returns>
    IHttpClient CreateClient();

    /// <summary>
    /// Creates a new <see cref="IHttpClient"/> instance using the provided <paramref name="username"/> and <paramref name="password"/>.
    /// </summary>
    /// <param name="username">The username to use for the credentials.</param>
    /// <param name="password">The password to use for the credentials.</param>
    /// <returns>A new <see cref="IHttpClient"/> instance with the configured HTTP <c>Authorization</c> header.</returns>
    IHttpClient CreateBasicClient(string username, string password);

    /// <summary>
    /// Creates a new <see cref="IHttpClient"/> instance using the provided bearer <paramref name="token"/>.
    /// </summary>
    /// <param name="token">The bearer token used as the credentials.</param>
    /// <returns>A new <see cref="IHttpClient"/> instance with the configured HTTP <c>Authorization</c> header.</returns>
    IHttpClient CreateBearerTokenClient(string token);

    /// <summary>
    /// Creates a new <see cref="IHttpClient"/> instance using the provided <paramref name="pat"/>.
    /// </summary>
    /// <param name="pat">The personal access token (PAT) used as the credentials.</param>
    /// <returns>A new <see cref="IHttpClient"/> instance with the configured HTTP <c>Authorization</c> header.</returns>
    IHttpClient CreatePersonalAccessTokenClient(string pat);
}
