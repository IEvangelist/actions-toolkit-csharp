#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using System.Text.Json.Serialization;

var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Supply GITHUB_TOKEN with contents:read.");
var context = GitHubApi.Context;
var client = GitHubApi.CreateClient(token);
var repository = await client.Repos[context.Repo.Owner][context.Repo.Repo].GetAsync()
    ?? throw new InvalidOperationException("GitHub did not return repository data.");
using var http = Http.CreateBearerTokenClient(token);
var server = Environment.GetEnvironmentVariable("GITHUB_API_URL") ?? "https://api.github.com";
var response = await http.GetAsync($"{server}/repos/{context.Repo.Owner}/{context.Repo.Repo}",
    RepositoryJson.Default.RepositoryInfo);
if (response.Result?.FullName != repository.FullName)
    throw new InvalidOperationException("GitHub and typed HTTP clients disagree.");
await Core.SetOutputAsync("repository", repository.FullName ?? throw new InvalidOperationException("Missing repository name."));
await Core.Summary.AddHeading("GitHub and typed HTTP").AddRaw($"Both clients identified `{repository.FullName}`.", true).WriteAsync();

public sealed record RepositoryInfo([property: JsonPropertyName("full_name")] string FullName);
[JsonSerializable(typeof(RepositoryInfo))]
internal partial class RepositoryJson : JsonSerializerContext;
