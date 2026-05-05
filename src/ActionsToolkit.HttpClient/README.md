# `ActionsToolkit.HttpClient` package

To install the [`ActionsToolkit.HttpClient`](https://www.nuget.org/packages/ActionsToolkit.HttpClient) NuGet package:

```xml
<PackageReference Include="ActionsToolkit.HttpClient" Version="[Version]" />
```

Or use the [`dotnet add package`](https://learn.microsoft.com/dotnet/core/tools/dotnet-add-package) .NET CLI command:

```bash
dotnet add package ActionsToolkit.HttpClient
```

## Get started

After installing the package, you can use the `IHttpClient` class to make HTTP requests:

```csharp
using ActionsToolkit.HttpClient;
using ActionsToolkit.HttpClient.Extensions;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

// Register services
var provider = new ServiceCollection()
    .AddHttpClientServices()
    .BuildServiceProvider();

// Get service from provider
var factory = provider.GetRequiredService<IHttpCredentialClientFactory>();

// Create HTTP client from factory.
using IHttpClient client = factory.CreateClient();

// Make request
TypedResponse<Todo> response = await client.GetAsync<Todo[]>(
    "https://jsonplaceholder.typicode.com/todos?userId=1&completed=false",
    Context.Default.TodoArray);

Console.WriteLine($"Status code: {response.StatusCode}");
Console.WriteLine($"Todo count: {response.Result.Length}");

public sealed record class Todo(
    int? UserId = null,
    int? Id = null,
    string? Title = null,
    bool? Completed = null);

[JsonSerializable(typeof(Todo[]))]
public sealed partial class Context : JsonSerializerContext { }
```

In this contrived example, you use the `IHttpClient` interface to make a GET request to the [JSONPlaceholder](https://jsonplaceholder.typicode.com/) API. You use the `TypedResponse<T>` class to deserialize the response into a `Todo` record.

## Authenticated requests

The package ships three credential handlers that mirror upstream `@actions/http-client`. Each can be used directly with `DefaultHttpClient`, or via the `IHttpCredentialClientFactory` overloads:

```csharp
using ActionsToolkit.HttpClient;
using ActionsToolkit.HttpClient.Handlers;

var factory = provider.GetRequiredService<IHttpCredentialClientFactory>();

// PAT (e.g. GITHUB_TOKEN) — encodes "PAT:<token>" as Basic auth.
using IHttpClient pat = factory.CreatePersonalAccessTokenClient(token);

// Bearer token — sets "Authorization: Bearer <token>".
using IHttpClient bearer = factory.CreateBearerTokenClient(token);

// HTTP Basic — sets "Authorization: Basic <base64(user:pass)>".
using IHttpClient basic = factory.CreateBasicClient(username, password);
```

For full control over a single outbound request, use `SendAsync(HttpRequestMessage)`. The configured credential handler still injects `Authorization`, but the body, query, headers, and verb are entirely yours:

```csharp
using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/widgets")
{
    Content = JsonContent.Create(payload, Context.Default.Widget),
};

using HttpResponseMessage response = await pat.SendAsync(request);
```

## Constants and error type

For parity with upstream the package exposes `HttpCodes`, `Headers`, `MediaTypes`, and `HttpClientError`:

```csharp
if ((int)response.StatusCode == HttpCodes.NotFound)
{
    throw new HttpClientError("Widget not found", response.StatusCode);
}
```

## Proxy

`Proxy.GetProxyUrl(uri)` and `Proxy.CheckBypass(uri)` honor the standard `HTTP_PROXY` / `HTTPS_PROXY` / `NO_PROXY` environment variables (with the same precedence rules as upstream). `Proxy.IsHttps(url)` is a small helper for selecting the right variable.

## Attribution

This package is a .NET port of the official [`@actions/http-client`](https://github.com/actions/toolkit/tree/main/packages/http-client) Node.js package, which ships under the [MIT license](https://github.com/actions/toolkit/blob/main/LICENSE.md). The .NET implementation aims to preserve the upstream public API surface (`HttpCodes`, `Headers`, `MediaTypes`, credential handlers, `Proxy`) and behavior (retry on 502/503/504 for safe verbs, 50-redirect cap, `NO_PROXY` precedence) where it makes sense in idiomatic .NET. Tests in `tests/ActionsToolkit.HttpClient.Tests` mirror the upstream `__tests__/*.test.ts` files file-for-file and use verbatim `it('…')` strings as `[Fact(DisplayName=…)]` so that upstream additions can be tracked over time.

