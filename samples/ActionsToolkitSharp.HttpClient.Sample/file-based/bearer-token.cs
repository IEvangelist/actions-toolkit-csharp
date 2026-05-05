// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// bearer-token.cs
//
// Companion to the upstream `@actions/http-client` README "Bearer support"
// note: https://github.com/actions/toolkit/tree/main/packages/http-client
//
// Shows the three credentialed factory methods that ActionsToolkitSharp.HttpClient
// exposes alongside the unauthenticated CreateClient():
//   * factory.CreateBearerTokenClient(token)
//   * factory.CreateBasicClient(username, password)
//   * factory.CreatePersonalAccessTokenClient(pat)
//
// Each one returns a fully-configured IHttpClient with an Authorization
// header pre-populated.
//
// Run with:
//   dotnet run bearer-token.cs

#:package ActionsToolkitSharp.HttpClient@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.HttpClient;
using ActionsToolkitSharp.HttpClient.Extensions;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddHttpClientServices()
    .BuildServiceProvider();

var factory = provider.GetRequiredService<IHttpCredentialClientFactory>();

// All three of these are valid — pick the one that matches your target
// API. For demonstration we just create the clients and dispose them.
using IHttpClient bearer = factory.CreateBearerTokenClient("ghs_demoBearerToken");
using IHttpClient basic = factory.CreateBasicClient("octocat", "hunter2");
using IHttpClient pat = factory.CreatePersonalAccessTokenClient("ghp_demoPersonalAccessToken");

Console.WriteLine("Created the following authenticated clients:");
Console.WriteLine($"  bearer: {bearer.GetType().Name}");
Console.WriteLine($"  basic : {basic.GetType().Name}");
Console.WriteLine($"  pat   : {pat.GetType().Name}");
