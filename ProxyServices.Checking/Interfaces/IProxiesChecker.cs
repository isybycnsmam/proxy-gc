using ProxyService.Core.Models;

namespace ProxyService.Checking.Interfaces
{
    public interface IProxiesChecker
    {
        public string Name { get; }
        public List<CheckingResult> CheckProxiesAsync(
            List<Proxy> proxies,
            CheckingMethod checkingMethod,
            CancellationToken cancellationToken);
    }
}
