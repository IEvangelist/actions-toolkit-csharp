# `ActionsToolkitSharp.HttpClient` package

To install the [`ActionsToolkitSharp.HttpClient`](https://www.nuget.org/packages/ActionsToolkitSharp.HttpClient) NuGet package:

```xml
<PackageReference Include="ActionsToolkitSharp.HttpClient" Version="[Version]" />
```

Or use the [`dotnet add package`](https://learn.microsoft.com/dotnet/core/tools/dotnet-add-package) .NET CLI command:

```bash
dotnet add package ActionsToolkitSharp.HttpClient
```

## Get started

After installing the package, you can use the `IHttpClient` class to make HTTP requests:

```csharp
using ActionsToolkitSharp.HttpClient;
using ActionsToolkitSharp.HttpClient.Extensions;
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
