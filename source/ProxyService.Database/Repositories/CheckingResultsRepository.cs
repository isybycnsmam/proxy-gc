using ProxyService.Core.Models;

namespace ProxyService.Database.Repositories;

public class CheckingResultsRepository(ProxiesDbContext dbContext)
{
    private readonly ProxiesDbContext _dbContext = dbContext;

    public async Task InsertCheckingResults(
        IEnumerable<CheckingResult> checkingResults,
        CancellationToken cancellationToken)
    {
        await _dbContext.CheckingResults.AddRangeAsync(checkingResults, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
