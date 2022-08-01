using ProxyService.Core.Models;
using ProxyService.Checking.Interfaces;
using ProxyService.Core.Services;
using System.Net;
using System.Diagnostics;

namespace ProxyService.Checking.Site
{
    public class SiteProxiesChecker : IProxiesChecker
    {
        public const int CHECKING_DEGREE_OF_PARALLELISM = 5;

        private readonly ProgressNotifierService _progressNotifierService;

        public string Name => "Site";

        public SiteProxiesChecker(
            ProgressNotifierService progressNotifierService)
        {
            _progressNotifierService = progressNotifierService;
        }

        public List<CheckingResult> CheckProxiesAsync(
            List<Proxy> proxies,
            CheckingMethod checkingMethod,
            CheckingMethodSession checkingSession,
            CancellationToken cancellationToken)
        {
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
                HttpWebRequest request = WebRequest.Create(checkingMethod.TestTarget) as HttpWebRequest;
                request.Timeout = 10000;
                
                if (proxy is not null)
                    request.Proxy = new WebProxy(proxy.Ip, proxy.Port);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var response = request.GetResponse();
                stopwatch.Stop();

                checkingResult.Result = true;
                checkingResult.ResponseTime = (int)stopwatch.ElapsedMilliseconds;
            }
            catch { }

            return checkingResult;
        }
    }
}
