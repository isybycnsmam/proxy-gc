using Microsoft.EntityFrameworkCore;

namespace ProxyService.Database.Repositories;

public class GettingMethodsRepository(ProxiesDbContext dbContext)
{
    private readonly ProxiesDbContext _dbContext = dbContext;

    public async Task<List<string>> GetActiveGettingMethodsNames(CancellationToken stoppingToken)
    {
        var activeGettingMethodsNames = await _dbContext.GettingMethods
            .Where(e => !e.IsDisabled)
            .Select(e => e.Name)
            .ToListAsync(stoppingToken);

        return activeGettingMethodsNames;
    }
}
