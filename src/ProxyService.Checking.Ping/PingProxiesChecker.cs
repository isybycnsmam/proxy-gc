using ProxyService.Core.Models;
using ProxyService.Checking.Interfaces;
using System.Net.NetworkInformation;
using System.Text;
using ProxyService.Core.Services;

namespace ProxyService.Checking.Ping
{
    public class PingProxiesChecker : IProxiesChecker
    {
        public const int CHECKING_DEGREE_OF_PARALLELISM = 5;

        private readonly ProgressNotifierService _progressNotifierService;

        public string Name => "Ping";

        public PingProxiesChecker(
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
                var pingSender = new System.Net.NetworkInformation.Ping();
                var data = "abcdefghijklmnoprstuwxyz12345678";
                var buffer = Encoding.ASCII.GetBytes(data);
                var options = new PingOptions(64, true);

                if (proxy is null)
                    proxy = new Proxy() { Ip = "localhost" };

                var reply = pingSender.Send(proxy.Ip, timeout: 10000, buffer, options);

                if (reply.Status == IPStatus.Success)
                {
                    checkingResult.Result = true;
                    checkingResult.ResponseTime = Convert.ToInt32(reply.RoundtripTime);
                }
            }
            catch { }

            return checkingResult;
        }
    }
}
