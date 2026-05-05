// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Attest.Tests;

/// <summary>
/// Common fixtures shared across the parity-port test suite. Mirrors the
/// upstream <c>__tests__/test-data.ts</c>-style helpers.
/// </summary>
internal static class TestData
{
    public const string SubjectName = "subjective";
    public const string SubjectDigestSha = "7d070f6b64d9bcc530fe99cc21eaaa4b3c364e0b2d367d7735671fa202a03b32";

    public static IReadOnlyDictionary<string, string> SubjectDigest => new Dictionary<string, string>
    {
        ["sha256"] = SubjectDigestSha,
    };

    public static Subject Subject => new()
    {
        Name = SubjectName,
        Digest = SubjectDigest,
    };

    public const string PredicateType = "https://example.com/predicate/v1";

    public static JsonNode PredicateParams => JsonNode.Parse("""{ "key": "value" }""")!;

    public static Predicate Predicate => new()
    {
        Type = PredicateType,
        Params = PredicateParams,
    };

    public static string EncodeFakeJwt(JsonObject payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        static string Base64Url(byte[] bytes)
        {
            var s = Convert.ToBase64String(bytes);
            return s.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        var header = Base64Url(Encoding.UTF8.GetBytes("""{"alg":"none"}"""));
        var body = Base64Url(Encoding.UTF8.GetBytes(payload.ToJsonString()));
        var sig = Base64Url(Encoding.UTF8.GetBytes("sig"));
        return $"{header}.{body}.{sig}";
    }

    public static JsonObject DefaultClaims() => new()
    {
        ["iss"] = "https://token.actions.githubusercontent.com",
        ["aud"] = "nobody",
        ["repository"] = "owner/repo",
        ["repository_id"] = "1",
        ["repository_owner_id"] = "2",
        ["workflow_ref"] = "owner/repo/.github/workflows/main.yml@refs/heads/main",
        ["job_workflow_ref"] = "owner/repo/.github/workflows/main.yml@refs/heads/main",
        ["ref"] = "refs/heads/main",
        ["sha"] = "babca52ab0c93ae16539e5923cb0d7403b9a093b",
        ["event_name"] = "push",
        ["run_id"] = "12345",
        ["run_attempt"] = "1",
        ["runner_environment"] = "github-hosted",
    };
}
