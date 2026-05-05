// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using ActionsToolkitSharp.Octokit;
using GitHub.Models;

var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("The GITHUB_TOKEN environment variable is required.");

var client = GitHubClientFactory.Create(token);

var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);

var user = "IEvangelist";

var events = await GetEventsAsync(user).ToListAsync();

// Select events, grouping on repo and ordering by creation dates
var groupedEvents = events?.GroupBy(e => e?.Repo?.Name)
    .Select(e => (e.Key, e.OrderBy(ed => ed?.CreatedAt).ToArray()));

foreach (var (repo, eventArray) in groupedEvents ?? Enumerable.Empty<(string?, Event?[])>())
{
    Console.WriteLine($"In {repo}");

    for (var i = 0; i < eventArray.Length; i++)
    {
        var @event = eventArray[i];
        if (@event is null or { Payload: null })
        {
            continue;
        }

        var line = GetEventBulletPointText(@event);

        Console.WriteLine(line);
    }
}

async IAsyncEnumerable<Event?> GetEventsAsync(string user)
{
    var page = 1;
    var done = false;

    while (!done)
    {
        var events = await client.Users[user].Events.GetAsync(config =>
        {
            config.QueryParameters.PerPage = 100;
            config.QueryParameters.Page = page;
        });

        if (events is null or { Count: 0 })
        {
            done = true;
            yield break;
        }

        foreach (var @event in events)
        {
            if (@event is null)
            {
                continue;
            }

            if (@event.CreatedAt < sevenDaysAgo)
            {
                yield return @event;
            }
            else
            {
                done = true;
            }
        }

        page++;
    }
}

string GetEventBulletPointText(Event @event)
{
    if (@event is null or { Payload: null })
    {
        return "";
    }

    var payload = @event.Payload;

    var url = payload.Comment?.HtmlUrl ?? payload.Issue?.HtmlUrl;
    var details = "TODO: get the details";

    return $"- [{@event.Type}: {payload.Action} {details}]({url})";
}
