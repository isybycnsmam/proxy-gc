using ProxyService.Core.Models;

namespace ProxyService.Checking.Interfaces;

public interface IProxiesChecker
{
    string Name { get; }

    CheckingResult TestProxy(
        Proxy? proxy,
        CheckingMethod checkingMethod,
        int checkingSessionId);
}
