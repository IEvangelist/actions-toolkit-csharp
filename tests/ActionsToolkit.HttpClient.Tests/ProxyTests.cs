// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient.Tests;

/// <summary>
/// C# port of <see href="https://github.com/actions/toolkit/blob/main/packages/http-client/__tests__/proxy.test.ts"/>.
/// Each <see cref="FactAttribute.DisplayName"/> mirrors the upstream
/// <c>it('…')</c> description verbatim for grep-from-upstream traceability.
/// </summary>
[Trait("Category", "RequiresEnvVar")]
public sealed class ProxyTests : IDisposable
{
    public ProxyTests()
    {
        Environment.SetEnvironmentVariable("no_proxy", null);
        Environment.SetEnvironmentVariable("http_proxy", null);
        Environment.SetEnvironmentVariable("https_proxy", null);
    }

    [Fact(DisplayName = "getProxyUrl does not return proxyUrl if variables not set")]
    public void GetProxyUrlDoesNotReturnUrlWhenVariablesUnset()
    {
        var proxyUrl = Proxy.GetProxyUrl(new Uri("https://github.com"));

        Assert.Null(proxyUrl);
    }

    [Fact(DisplayName = "getProxyUrl returns proxyUrl if https_proxy set for https url")]
    public void GetProxyUrlReturnsUrlWhenHttpsProxySet()
    {
        Environment.SetEnvironmentVariable("https_proxy", "https://myproxysvr");

        var proxyUrl = Proxy.GetProxyUrl(new Uri("https://github.com"));

        Assert.NotNull(proxyUrl);
    }

    [Fact(DisplayName = "getProxyUrl does not return proxyUrl if http_proxy set for https url")]
    public void GetProxyUrlDoesNotReturnUrlWhenHttpsProxyUnset()
    {
        Environment.SetEnvironmentVariable("http_proxy", "https://myproxysvr");

        var proxyUrl = Proxy.GetProxyUrl(new Uri("https://github.com"));

        Assert.Null(proxyUrl);
    }

    [Fact(DisplayName = "getProxyUrl returns proxyUrl if http_proxy set for http url")]
    public void GetProxyUrlReturnsUrlWhenHttpProxySet()
    {
        Environment.SetEnvironmentVariable("http_proxy", "http://myproxysvr");

        var proxyUrl = Proxy.GetProxyUrl(new Uri("http://github.com"));

        Assert.NotNull(proxyUrl);
    }

    [Fact(DisplayName = "getProxyUrl does not return proxyUrl if https_proxy set and in no_proxy list")]
    public void GetProxyUrlDoesNotReturnUrlWhenHttpsProxySetAndInNoProxyList()
    {
        Environment.SetEnvironmentVariable("https_proxy", "https://myproxysvr");
        Environment.SetEnvironmentVariable("no_proxy", "otherserver,myserver,anotherserver:8080");

        var proxyUrl = Proxy.GetProxyUrl(new Uri("https://myserver"));

        Assert.Null(proxyUrl);
    }

    [Fact(DisplayName = "getProxyUrl returns proxyUrl if https_proxy set and not in no_proxy list")]
    public void GetProxyUrlDoesNotReturnUrlWhenHttpsProxySetAndNotInNoProxyList()
    {
        Environment.SetEnvironmentVariable("https_proxy", "https://myproxysvr");
        Environment.SetEnvironmentVariable("no_proxy", "otherserver,myserver,anotherserver:8080");

        var proxyUrl = Proxy.GetProxyUrl(new Uri("https://github.com"));

        Assert.NotNull(proxyUrl);
    }

    [Fact(DisplayName = "getProxyUrl does not return proxyUrl if http_proxy set and in no_proxy list")]
    public void GetProxyUrlDoesNotReturnUrlWhenHttpProxySetAndInNoProxyList()
    {
        Environment.SetEnvironmentVariable("http_proxy", "http://myproxysvr");
        Environment.SetEnvironmentVariable("no_proxy", "otherserver,myserver,anotherserver:8080");

        var proxyUrl = Proxy.GetProxyUrl(new Uri("http://myserver"));

        Assert.Null(proxyUrl);
    }

    [Fact(DisplayName = "getProxyUrl returns proxyUrl if http_proxy set and not in no_proxy list")]
    public void GetProxyUrlDoesNotReturnUrlWhenHttpProxySetAndNotInNoProxyList()
    {
        Environment.SetEnvironmentVariable("http_proxy", "http://myproxysvr");
        Environment.SetEnvironmentVariable("no_proxy", "otherserver,myserver,anotherserver:8080");

        var proxyUrl = Proxy.GetProxyUrl(new Uri("http://github.com"));

        Assert.NotNull(proxyUrl);
    }

    [Fact(DisplayName = "getProxyUrl returns proxyUrl if http_proxy has no protocol")]
    public void GetProxyUrlReturnsUrlWhenHttpProxyHasNoProtocol()
    {
        Environment.SetEnvironmentVariable("http_proxy", "myproxysvr");

        var proxyUrl = Proxy.GetProxyUrl(new Uri("http://github.com"));

        Assert.Equal("http://myproxysvr/", proxyUrl!.ToString());
    }

    [Fact(DisplayName = "checkBypass returns true if host as no_proxy list")]
    public void CheckBypassReturnsTrueWhenHostIsNoProxyList()
    {
        Environment.SetEnvironmentVariable("no_proxy", "myserver");

        var bypass = Proxy.CheckBypass(new Uri("https://myserver"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if host in no_proxy list")]
    public void CheckBypassReturnsTrueWhenHostInNoProxyList()
    {
        Environment.SetEnvironmentVariable("no_proxy", "otherserver,myserver,anotherserver:8080");

        var bypass = Proxy.CheckBypass(new Uri("https://myserver"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if host in no_proxy list with spaces")]
    public void CheckBypassReturnsTrueWhenHostInNoProxyListWithSpaces()
    {
        Environment.SetEnvironmentVariable("no_proxy", "otherserver, myserver ,anotherserver:8080");

        var bypass = Proxy.CheckBypass(new Uri("https://myserver"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if host in no_proxy list with port")]
    public void CheckBypassReturnsTrueWhenHostInNoProxyListWithPort()
    {
        Environment.SetEnvironmentVariable("no_proxy", "otherserver, myserver:8080 ,anotherserver");

        var bypass = Proxy.CheckBypass(new Uri("https://myserver:8080"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if host with port in no_proxy list without port")]
    public void CheckBypassReturnsTrueWhenHostInNoProxyListWithoutPort()
    {
        Environment.SetEnvironmentVariable("no_proxy", "otherserver, myserver ,anotherserver");

        var bypass = Proxy.CheckBypass(new Uri("https://myserver:8080"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if host in no_proxy list with default https port")]
    public void CheckBypassReturnsTrueWhenHostInNoProxyListWithHttpsPort()
    {
        Environment.SetEnvironmentVariable("no_proxy", "otherserver, myserver:443 ,anotherserver");

        var bypass = Proxy.CheckBypass(new Uri("https://myserver"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if host in no_proxy list with default http port")]
    public void CheckBypassReturnsTrueWhenHostInNoProxyListWithHttpPort()
    {
        Environment.SetEnvironmentVariable("no_proxy", "otherserver, myserver:80 ,anotherserver");

        var bypass = Proxy.CheckBypass(new Uri("http://myserver"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns false if host not in no_proxy list")]
    public void CheckBypassReturnsFalseWhenHostNotInNoProxyList()
    {
        Environment.SetEnvironmentVariable("no_proxy", "otherserver, myserver ,anotherserver:8080");

        var bypass = Proxy.CheckBypass(new Uri("https://github.com"));

        Assert.False(bypass);
    }

    [Fact(DisplayName = "checkBypass returns false if empty no_proxy")]
    public void CheckBypassReturnsFalseWhenNoProxyEmpty()
    {
        Environment.SetEnvironmentVariable("no_proxy", "");

        var bypass = Proxy.CheckBypass(new Uri("https://github.com"));

        Assert.False(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if host with subdomain in no_proxy")]
    public void CheckBypassReturnsTrueWhenHostWithSubdomainInNoProxy()
    {
        Environment.SetEnvironmentVariable("no_proxy", "myserver.com");

        var bypass = Proxy.CheckBypass(new Uri("https://sub.myserver.com"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns false if no_proxy is subdomain")]
    public void CheckBypassReturnsFalseWhenNoProxyIsSubdomain()
    {
        Environment.SetEnvironmentVariable("no_proxy", "myserver.com");

        var bypass = Proxy.CheckBypass(new Uri("https://myserver.com.evil.org"));

        Assert.False(bypass);
    }

    [Fact(DisplayName = "checkBypass returns false if no_proxy is part of domain")]
    public void CheckBypassReturnsFalseWhenNoProxyIsPartOfDomain()
    {
        Environment.SetEnvironmentVariable("no_proxy", "myserver.com");

        var bypass = Proxy.CheckBypass(new Uri("https://evilmyserver.com"));

        Assert.False(bypass);
    }

    [Fact(DisplayName = "checkBypass returns false if host with leading dot in no_proxy")]
    public void CheckBypassReturnsFalseWhenHostWithLeadingDotInNoProxy()
    {
        Environment.SetEnvironmentVariable("no_proxy", ".myserver.com");

        var bypass = Proxy.CheckBypass(new Uri("https://myserver.com"));

        Assert.False(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if host with subdomain in no_proxy defined with leading \".\"")]
    public void CheckBypassReturnsTrueWhenHostWithSubdomainInNoProxyWithLeadingDot()
    {
        Environment.SetEnvironmentVariable("no_proxy", ".myserver.com");

        var bypass = Proxy.CheckBypass(new Uri("https://sub.myserver.com"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if no_proxy is \"*\"")]
    public void CheckBypassReturnsTrueWhenNoProxyIsWildcard()
    {
        Environment.SetEnvironmentVariable("no_proxy", "*");

        var bypass = Proxy.CheckBypass(new Uri("https://anything.whatsoever.com"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "checkBypass returns true if no_proxy contains comma separated \"*\"")]
    public void CheckBypassReturnsTrueWhenNoProxyContainsCommaSeparatedWildcard()
    {
        Environment.SetEnvironmentVariable("no_proxy", "domain.com,* , example.com");

        var bypass = Proxy.CheckBypass(new Uri("https://anything.whatsoever.com"));

        Assert.True(bypass);
    }

    [Fact(DisplayName = "isHttps returns true for https url and false for http url")]
    public void IsHttpsReturnsExpectedSchemeFlag()
    {
        Assert.True(Proxy.IsHttps("https://github.com"));
        Assert.False(Proxy.IsHttps("http://github.com"));
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("no_proxy", null);
        Environment.SetEnvironmentVariable("http_proxy", null);
        Environment.SetEnvironmentVariable("https_proxy", null);
    }
}
