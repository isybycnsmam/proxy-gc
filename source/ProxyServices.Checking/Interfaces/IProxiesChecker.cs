using ProxyService.Core.Models;

namespace ProxyService.Checking.Interfaces
{
    public interface IProxiesChecker
    {
        public string Name { get; }

        public List<CheckingResult> CheckProxiesAsync(
            List<Proxy> proxies,
            CheckingMethod checkingMethod,
            CheckingMethodSession checkingSession,
            CancellationToken cancellationToken);

        public CheckingResult TestProxy(
            Proxy proxy,
            CheckingMethod checkingMethod,
            int checkingSessionId);
    }
}
