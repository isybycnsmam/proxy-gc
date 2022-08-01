using Microsoft.Extensions.Logging;
using ProxyService.Core.Enums;
using ProxyService.Core.Models;
using System.Text.RegularExpressions;
using ProxyService.Getting.Interfaces;
using System.Text;

namespace ProxyService.Getting.HttpsSpysOne
{
    public sealed class HttpsSpysOneProxiesGetter : IProxiesGetter
    {
        private const string SINGLE_PROXY_REGEX = @"(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(?<port>\d+).*?(?<anonymity>NOA|HIA|ANM).*?\/free-proxy-list\/(?<countryCode>[A-Z]+)";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpsSpysOneProxiesGetter> _logger;

        public string Name => "HttpsSpysOne";

        public HttpsSpysOneProxiesGetter(
            IHttpClientFactory httpClientFactory,
            ILogger<HttpsSpysOneProxiesGetter> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<Proxy>> GetProxiesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting spys one https proxies from html page");
            var response = await GetPageSourceHtmlAsync(cancellationToken);
            response = FuckingKeyDecryptor.DecryptPortNumbers(response);
            var matches = Regex.Matches(response, SINGLE_PROXY_REGEX, RegexOptions.None, TimeSpan.FromSeconds(10));
            var proxyList = new List<Proxy>();
            foreach (Match match in matches)
            {
                var ip = match.Groups["ip"]?.Value;
                var port = match.Groups["port"]?.Value;
                var countryCode = match.Groups["countryCode"]?.Value;
                var anonymity = match.Groups["anonymity"]?.Value;

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
                    Type = ProxyType.Https,
                });
            }
            return proxyList;
        }

        private async Task<string> GetPageSourceHtmlAsync(CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            var randomNumber = Guid.NewGuid().ToString("N");
            var postData = $"xx0={randomNumber}&xpp=5&xf1=0&xf4=0&xf5=0";
            var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://spys.one/en/https-ssl-proxy/") { Content = content };
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");
            var response = await client.SendAsync(request, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseString;
        }

        private ProxyAnonymity ConvertStringToAnonymity(string source)
        {
            if (source == "ANM")
                return ProxyAnonymity.Anonymous;
            else if (source == "HIA")
                return ProxyAnonymity.HighAnonymous;
            else
                return ProxyAnonymity.None;
        }
    }
}
