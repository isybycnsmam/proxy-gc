using Microsoft.EntityFrameworkCore;
using ProxyService.Core.Models;

namespace ProxyService.Database.Repositories;

public class CheckingSessionsRepository(ProxiesDbContext dbContext)
{
    private readonly ProxiesDbContext _dbContext = dbContext;

    public async Task<CheckingSession> CreateCheckingSession(
        int checkingRunId,
        CheckingMethod checkingMethod,
        CancellationToken cancellationToken)
    {
        var checkingMethodSession = new CheckingSession()
        {
            CheckingMethodId = checkingMethod.Id,
            CheckingRunId = checkingRunId
        };

        await _dbContext.CheckingSessions.AddAsync(checkingMethodSession, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return checkingMethodSession;
    }

    public async Task UpdateElapsed(
        int checkingMethodSessionId,
        long elapsedMilliseconds,
        CancellationToken cancellationToken)
    {
        var checkingMethodSession = await _dbContext.CheckingSessions
            .FirstAsync(e => e.Id == checkingMethodSessionId, cancellationToken);
        checkingMethodSession.Elapsed = (int)elapsedMilliseconds;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
