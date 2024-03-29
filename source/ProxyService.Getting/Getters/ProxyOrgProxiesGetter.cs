using Microsoft.Extensions.Logging;
using ProxyService.Core.Enums;
using ProxyService.Core.Models;
using ProxyService.Getting.Interfaces;
using System.Text.RegularExpressions;

namespace ProxyService.Getting.ProxyOrg;

public class ProxyOrgProxiesGetter(
    IHttpClientFactory httpClientFactory,
    ILogger<ProxyOrgProxiesGetter> logger) : IProxiesGetter
{
    public string Name => "ProxyOrg";
    public readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    public readonly ILogger<ProxyOrgProxiesGetter> _logger = logger;

    private const string SINGLE_PROXY_REGEX = @"(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}).{9}(?<port>\d+).{9}(?<countryCode>[A-Z]{2}).{20}([A-Za-z ]+).{9}(?<anonymity>anonymous|elite proxy|transparent).*?hx'>(?<ssl>yes|no)";
    private static readonly List<string> _subSites = [
        "https://free-proxy-list.net/",
        "https://free-proxy-list.net/uk-proxy.html",
        "https://www.us-proxy.org/",
        "https://www.sslproxies.org/",
    ];

    public async Task<List<Proxy>> GetProxiesAsync(CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();

        var proxyList = new List<Proxy>();
        foreach (var site in _subSites)
        {
            _logger.LogInformation("Getting proxies from ProxyOrg sub-page {siteUrl}", site);
            var response = await client.GetStringAsync(site, cancellationToken);
            var matches = Regex.Matches(response, SINGLE_PROXY_REGEX, RegexOptions.None, TimeSpan.FromSeconds(10));
            _logger.LogInformation("Successfully downloaded {count} proxies from ProxyOrg sub-page {siteUrl}", matches.Count, site);

            foreach (Match match in matches)
            {
                var ip = match.Groups["ip"]?.Value;
                var port = match.Groups["port"]?.Value;
                var countryCode = match.Groups["countryCode"]?.Value;
                var anonymity = match.Groups["anonymity"]?.Value;
                var isSsl = match.Groups["ssl"]?.Value == "yes";

                if (string.IsNullOrEmpty(ip) ||
                    string.IsNullOrEmpty(port) ||
                    string.IsNullOrEmpty(countryCode) ||
                    string.IsNullOrEmpty(anonymity))
                {
                    _logger.LogWarning("One of proxy properties is null or empty. {match}", match.Value);
                    continue;
                }

                proxyList.Add(new Proxy()
                {
                    Ip = ip,
                    Port = Convert.ToInt32(port),
                    CountryCode = countryCode,
                    Anonymity = ConvertStringToAnonymity(anonymity),
                    Type = isSsl ? ProxyType.Https : ProxyType.Http,
                });
            }
        }

        var uniqueProxies = proxyList
            .GroupBy(e => e.IpPort)
            .Select(proxy => proxy.First());

        return uniqueProxies.ToList();
    }

    private static ProxyAnonymity ConvertStringToAnonymity(string source)
    {
        return source.ToLower() switch
        {
            "anonymous" => ProxyAnonymity.Anonymous,
            "elite proxy" => ProxyAnonymity.HighAnonymous,
            _ => ProxyAnonymity.None,
        };
    }
}