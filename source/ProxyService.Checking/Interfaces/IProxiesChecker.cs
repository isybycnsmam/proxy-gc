using ProxyService.Core.Models;

namespace ProxyService.Checking.Interfaces
{
    public interface IProxiesChecker
    {
        string Name { get; }

        List<CheckingResult> CheckProxiesAsync(
            List<Proxy> proxies,
            CheckingMethod checkingMethod,
            CheckingMethodSession checkingSession,
            CancellationToken cancellationToken);

        CheckingResult TestProxy(
            Proxy proxy,
            CheckingMethod checkingMethod,
            int checkingSessionId);
    }
}
