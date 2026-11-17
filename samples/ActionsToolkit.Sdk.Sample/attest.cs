#!/usr/bin/env -S dotnet --
#:sdk ActionsToolkit.Sdk

using System.Security.Cryptography;
using ActionsToolkit.Attest.Models;

var subject = Path.GetFullPath(".showcase/native/aot-app");
var digest = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(subject))).ToLowerInvariant();
var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Supply GITHUB_TOKEN with attestations:write and id-token:write.");
using var attest = Attest.CreateClient();
await attest.AttestProvenanceAsync(new AttestProvenanceOptions
{
    Token = token,
    Subjects = [new Subject { Name = "aot-app", Digest = new Dictionary<string, string> { ["sha256"] = digest } }]
});
await Core.SetOutputAsync("sha256", digest);
await Core.Summary.AddMarkdownHeading("Native executable attested").AddNewLine()
    .AddRawMarkdown($"Subject `aot-app`, SHA-256 `{digest}`. The next step independently verifies provenance.", true).WriteAsync();
