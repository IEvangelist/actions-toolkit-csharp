// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// get-todos.cs
//
// Mirrors the upstream `@actions/http-client` README "Get started" example
// — but ported to the C# API exposed by ActionsToolkit.HttpClient:
// https://github.com/actions/toolkit/tree/main/packages/http-client
//
// Demonstrates:
//   * Registering services with AddHttpClientServices()
//   * Resolving the IHttpCredentialClientFactory
//   * Issuing a typed GET request that deserializes via a
//     System.Text.Json source-generated context (NativeAOT-friendly).
//
// Run with:
//   dotnet run get-todos.cs

#:package ActionsToolkit.HttpClient@*
#:package Microsoft.Extensions.DependencyInjection@*

using System.Text.Json.Serialization;
using ActionsToolkit.HttpClient;
using ActionsToolkit.HttpClient.Extensions;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddHttpClientServices()
    .BuildServiceProvider();

var factory = provider.GetRequiredService<IHttpCredentialClientFactory>();

using IHttpClient client = factory.CreateClient();

TypedResponse<Todo[]> response = await client.GetAsync(
    "https://jsonplaceholder.typicode.com/todos?userId=1&completed=false",
    TodoJsonContext.Default.TodoArray);

Console.WriteLine($"Status code: {response.StatusCode}");
Console.WriteLine($"Todo count : {response.Result?.Length ?? 0}");

foreach (var todo in (response.Result ?? []).Take(3))
{
    Console.WriteLine($"  #{todo.Id} ({(todo.Completed == true ? "x" : " ")}) {todo.Title}");
}

public sealed record class Todo(
    int? UserId = null,
    int? Id = null,
    string? Title = null,
    bool? Completed = null);

[JsonSerializable(typeof(Todo[]))]
public sealed partial class TodoJsonContext : JsonSerializerContext;
