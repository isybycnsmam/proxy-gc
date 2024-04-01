using ProxyService.Core.Models;

namespace ProxyService.Getting.Interfaces;

public interface IProxiesGetter
{
    abstract string Name { get; }
    Task<List<Proxy>> GetProxiesAsync(CancellationToken cancellationToken);
}
