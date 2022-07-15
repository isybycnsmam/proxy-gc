using ProxyService.Core.Models;
using System.Text.RegularExpressions;

namespace ProxyService.Getting.Interfaces
{
    public interface IProxiesGetter
    {
        public abstract string Name { get; }
        public Task<List<Proxy>> GetProxiesAsync(CancellationToken cancellationToken);
    }
}
