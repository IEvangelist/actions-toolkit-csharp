// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient;

/// <summary>
/// Provides proxy configuration utilities that consult the standard
/// <c>http_proxy</c>, <c>https_proxy</c>, and <c>no_proxy</c>
/// environment variables. Mirrors the upstream <c>proxy</c> module from
/// <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/src/proxy.ts">@actions/http-client</see>.
/// </summary>
public static class Proxy
{
    /// <summary>
    /// Returns the configured proxy <see cref="Uri"/> for the given
    /// <paramref name="requestUrl"/>, or <see langword="null"/> if no proxy
    /// applies (either none is configured or the host is in the bypass list).
    /// </summary>
    /// <param name="requestUrl">The request URL whose scheme is used to choose
    /// between <c>https_proxy</c> and <c>http_proxy</c>.</param>
    /// <returns>The resolved proxy URI, or <see langword="null"/> when no proxy applies.</returns>
    public static Uri? GetProxyUrl(Uri requestUrl)
    {
        ArgumentNullException.ThrowIfNull(requestUrl);

        var usingSsl = requestUrl.Scheme is "https";

        if (CheckBypass(requestUrl))
        {
            return null;
        }

        var proxyVar = GetProxyVariable(usingSsl);

        static string? GetProxyVariable(bool usingSsl)
        {
            return usingSsl
                ? Environment.GetEnvironmentVariable("https_proxy")
                ?? Environment.GetEnvironmentVariable("HTTPS_PROXY")
                : Environment.GetEnvironmentVariable("http_proxy")
                ?? Environment.GetEnvironmentVariable("HTTP_PROXY");
        }

        if (proxyVar is not null)
        {
            try
            {
                return new Uri(proxyVar);
            }
            catch
            {
                if (proxyVar.StartsWith("http://", StringComparison.OrdinalIgnoreCase) is false &&
                    proxyVar.StartsWith("https://", StringComparison.OrdinalIgnoreCase) is false)
                {
                    return new Uri($"http://{proxyVar}");
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether the supplied <paramref name="requestUrl"/> should
    /// bypass any configured proxy because its host matches an entry in the
    /// <c>no_proxy</c> environment variable, is a loopback address, or
    /// <c>no_proxy</c> contains <c>"*"</c>.
    /// </summary>
    /// <param name="requestUrl">The request URL to test.</param>
    /// <returns><see langword="true"/> when the request should bypass the proxy.</returns>
    public static bool CheckBypass(Uri requestUrl)
    {
        ArgumentNullException.ThrowIfNull(requestUrl);

        var hostName = requestUrl.Host;

        if (hostName is null)
        {
            return false;
        }

        if (requestUrl.IsLoopback)
        {
            return true;
        }

        var noProxy = Environment.GetEnvironmentVariable("no_proxy")
            ?? Environment.GetEnvironmentVariable("NO_PROXY");

        if (noProxy is null)
        {
            return false;
        }

        string[] upperReqHosts =
        [
            requestUrl.Host.ToUpperInvariant(),
            $"{requestUrl.Host.ToUpperInvariant()}:{requestUrl.Port}"
        ];

        var upperNoProxyItems = noProxy.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToUpperInvariant())
            .Where(x => !string.IsNullOrEmpty(x));

        foreach (var upperNoProxyItem in upperNoProxyItems)
        {
            if (upperNoProxyItem is "*" ||
                upperReqHosts.Any(x => x == upperNoProxyItem ||
                    x.EndsWith($".{upperNoProxyItem}", StringComparison.OrdinalIgnoreCase) ||
                (upperNoProxyItem.StartsWith('.') && x.EndsWith(upperNoProxyItem, StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the supplied <paramref name="requestUrl"/>
    /// uses the <c>https</c> scheme. Mirrors the upstream <c>isHttps</c> helper.
    /// </summary>
    /// <param name="requestUrl">The request URL to inspect.</param>
    /// <returns><see langword="true"/> if the URL is HTTPS; otherwise <see langword="false"/>.</returns>
    public static bool IsHttps(string requestUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUrl);

        return new Uri(requestUrl).Scheme is "https";
    }
}
