# `ActionsToolkit.HttpClient` — file-based examples

These single-file scripts use .NET 10's
[`dotnet run app.cs`](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
support to demonstrate the API exposed by
[`ActionsToolkit.HttpClient`](https://www.nuget.org/packages/ActionsToolkit.HttpClient).

The package wraps `Microsoft.Extensions.Http` (with the standard
resilience handler) and exposes an `IHttpClient` abstraction whose
typed `GetAsync<T>` / `PostAsync<TData,TResult>` / etc. methods take a
`JsonTypeInfo<T>` so they are fully NativeAOT-friendly.

The `#:package ActionsToolkit.HttpClient@*` directive at the top of
each script declares an inline NuGet reference, so no `.csproj` is
required — just `dotnet run <file>.cs`.

| File | Demonstrates | Run command |
| --- | --- | --- |
| [`get-todos.cs`](./get-todos.cs) | `factory.CreateClient()` + `client.GetAsync<Todo[]>(uri, jsonTypeInfo)` (mirrors the upstream README quick-start) | `dotnet run get-todos.cs` |
| [`bearer-token.cs`](./bearer-token.cs) | `CreateBearerTokenClient(token)`, `CreateBasicClient(user, pass)`, `CreatePersonalAccessTokenClient(pat)` | `dotnet run bearer-token.cs` |

## Run them all

[`run-all.sh`](./run-all.sh) drives every example in sequence with
`set -euo pipefail`. The `get-todos.cs` script makes a network request
to `jsonplaceholder.typicode.com` so it requires outbound network
access; on offline runners you can skip it.

```bash
chmod +x run-all.sh
./run-all.sh
```

## Use from a workflow

[`usage.yml`](./usage.yml) shows how to invoke a file-based HttpClient
script from a real workflow `run:` step.
