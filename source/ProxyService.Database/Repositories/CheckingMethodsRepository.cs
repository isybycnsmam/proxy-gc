using Microsoft.EntityFrameworkCore;
using ProxyService.Core.Models;

namespace ProxyService.Database.Repositories;

public class CheckingMethodsRepository(ProxiesDbContext dbContext)
{
    private readonly ProxiesDbContext _dbContext = dbContext;

    public async Task<List<CheckingMethod>> GetActiveCheckingMethods(CancellationToken stoppingToken)
    {
        var checkingMethods = await _dbContext.CheckingMethods
            .AsNoTracking()
            .Where(method => !method.IsDisabled)
            .ToListAsync(stoppingToken);

        return checkingMethods;
    }
}
