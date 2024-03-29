using Microsoft.EntityFrameworkCore;
using ProxyService.Core.Models;

namespace ProxyService.Database.Repositories;

public class ProxiesRepository(ProxiesDbContext dbContext)
{
    private readonly ProxiesDbContext _dbContext = dbContext;

    /// <summary>
    /// Inserts only new proxies to the database
    /// </summary>
    /// <returns>Number of unique proxies that has been added to the database</returns>
    public async Task<int> InsertNewProxies(IEnumerable<Proxy> newProxies, CancellationToken stoppingToken)
    {
        var existingProxies = await _dbContext.Proxies.AsNoTracking().ToListAsync(stoppingToken);
        var proxiesToAdd = FilterProxiesToAdd(newProxies, existingProxies);
        await _dbContext.Proxies.AddRangeAsync(proxiesToAdd, stoppingToken);
        await _dbContext.SaveChangesAsync(stoppingToken);
        return proxiesToAdd.Count;
    }

    public async Task<List<Proxy>> GetProxiesToCheck(CancellationToken cancellationToken)
    {
        var weekInThePast = DateTime.Now.AddDays(-7);

        var proxiesToCheck = await _dbContext.Proxies
            .AsNoTracking()
            .Where(e =>
                e.IsDeleted == false ||
                e.LastChecked < weekInThePast)
            .ToListAsync(cancellationToken);

        return proxiesToCheck;
    }

    private static List<Proxy> FilterProxiesToAdd(IEnumerable<Proxy> newProxies, IEnumerable<Proxy> existingProxies)
    {
        var joinedProxies = from newProxy in newProxies
                            join existingProxy in existingProxies on newProxy.IpPort equals existingProxy.IpPort into gj
                            from subExistingProxy in gj.DefaultIfEmpty()
                            select new
                            {
                                Proxy = newProxy,
                                IsNew = subExistingProxy is null,
                            };

        return joinedProxies
            .Where(e => e.IsNew)
            .Select(e => e.Proxy)
            .ToList();
    }
}
