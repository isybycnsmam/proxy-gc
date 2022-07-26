using Microsoft.Extensions.Logging;
using ProxyService.Core.Enums;
using ProxyService.Core.Models;
using System.Text.RegularExpressions;
using ProxyService.Getting.Interfaces;

namespace ProxyService.Getting.TextSpysOne
{
    public sealed class TextSpysOneProxiesGetter : IProxiesGetter
    {
        private const string SINGLE_PROXY_REGEX = @"(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(?<port>\d+) (?<countryCode>[A-Z]+)-(?<anonymity>N|A|H)(!| |-)(?<ssl>S?)";
       
        public readonly IHttpClientFactory _httpClientFactory;
        public readonly ILogger<TextSpysOneProxiesGetter> _logger;
        
        public string Name => "TextSpysOne";

        public TextSpysOneProxiesGetter(
            IHttpClientFactory httpClientFactory,
            ILogger<TextSpysOneProxiesGetter> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<Proxy>> GetProxiesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting spys one http/s proxies from remote txt file");
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync("https://spys.me/proxy.txt", cancellationToken);
            var matches = Regex.Matches(response, SINGLE_PROXY_REGEX, RegexOptions.None, TimeSpan.FromSeconds(10));
            var proxyList = new List<Proxy>();
            foreach (Match match in matches)
            {
                var ip = match.Groups["ip"]?.Value;
                var port = match.Groups["port"]?.Value;
                var countryCode = match.Groups["countryCode"]?.Value;
                var anonymity = match.Groups["anonymity"]?.Value;
                var isSsl = !string.IsNullOrEmpty(match.Groups["ssl"]?.Value);

                if (string.IsNullOrEmpty(ip) ||
                    string.IsNullOrEmpty(port) ||
                    string.IsNullOrEmpty(countryCode) ||
                    string.IsNullOrEmpty(anonymity))
                {
                    _logger.LogWarning("One of proxy properties is null or empty. {0}", match.Value);
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
            return proxyList;
        }

        private ProxyAnonymity ConvertStringToAnonymity(string source)
        {
            if (source == "A")
                return ProxyAnonymity.Anonymous;
            else if (source == "H")
                return ProxyAnonymity.HighAnonymous;
            else
                return ProxyAnonymity.None;
        }
    }
}
