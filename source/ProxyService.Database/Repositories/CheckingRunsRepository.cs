using ProxyService.Core.Models;

namespace ProxyService.Database.Repositories;

public class CheckingRunsRepository(ProxiesDbContext dbContext)
{
    private readonly ProxiesDbContext _dbContext = dbContext;

    public async Task<CheckingRun> CreateCheckingRun(CancellationToken cancellationToken)
    {
        var checkingRun = new CheckingRun();
        await _dbContext.CheckingRuns.AddAsync(checkingRun, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return checkingRun;
    }
}