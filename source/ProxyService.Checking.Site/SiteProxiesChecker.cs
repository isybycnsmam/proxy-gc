using ProxyService.Core.Models;
using ProxyService.Checking.Interfaces;
using ProxyService.Core.Services;
using System.Net;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ProxyService.Checking.Site
{
    public class SiteProxiesChecker : IProxiesChecker
    {
        public const int CHECKING_DEGREE_OF_PARALLELISM = 5;

        private readonly ProgressNotifierService _progressNotifierService;
        private readonly ILogger<SiteProxiesChecker> _logger;

        public string Name => "Site";

        public SiteProxiesChecker(
            ILogger<SiteProxiesChecker> logger,
            ProgressNotifierService progressNotifierService)
        {
            _logger = logger;
            _progressNotifierService = progressNotifierService;
        }

        public List<CheckingResult> CheckProxiesAsync(
            List<Proxy> proxies,
            CheckingMethod checkingMethod,
            CheckingMethodSession checkingSession,
            CancellationToken cancellationToken)
        {

            if (string.IsNullOrEmpty(checkingMethod.TestTarget))
            {
                _logger.LogError("Invalid configuration {0}. TestTarget is null or empty", checkingMethod.Id);
                return new List<CheckingResult>();
            }

            _progressNotifierService.StartNotifying(
                message: $"Checking proxies {Name}({checkingMethod.Description}) progress: {{progress}}%",
                itemsCount: proxies.Count,
                delay: TimeSpan.FromSeconds(10));

            var checkingResults = proxies
                .AsParallel()
                .WithDegreeOfParallelism(CHECKING_DEGREE_OF_PARALLELISM)
                .WithCancellation(cancellationToken)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .Select(proxy => TestProxy(proxy, checkingMethod, checkingSession.Id))
                .Select(_progressNotifierService.ReportProgress)
                .ToList();

            _progressNotifierService.StopNotifying();

            return checkingResults;
        }

        public CheckingResult TestProxy(Proxy proxy, CheckingMethod checkingMethod, int checkingSessionId)
        {
            var checkingResult = new CheckingResult()
            {
                ProxyId = proxy?.Id ?? 0,
                CheckingMethodSessionId = checkingSessionId,
                Result = false,
                ResponseTime = 0,
            };

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(checkingMethod.TestTarget);
                request.Timeout = 10000;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                
                if (proxy is not null)
                    request.Proxy = new WebProxy(proxy.Ip, proxy.Port);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                using var response = request.GetResponse();
                stopwatch.Stop();

                checkingResult.Result = true;
                checkingResult.ResponseTime = (int)stopwatch.ElapsedMilliseconds;
            }
            catch { }

            return checkingResult;
        }
    }
}
