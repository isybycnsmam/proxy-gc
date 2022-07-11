using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProxyService.Database;
using ProxyService.Checking.Interfaces;
using Autofac.Features.OwnedInstances;

namespace ProxyService.Checking
{
    public class CheckingProxiesWorker : BackgroundService
    {
        private readonly TimeSpan RUN_DELAY = TimeSpan.FromHours(1);

        private readonly ILogger<CheckingProxiesWorker> _logger;
        private readonly IEnumerable<IProxiesChecker> _proxiesCheckers;
        private readonly Func<Owned<ProxiesDbContext>> _proxiesDbContextFactory;

        public CheckingProxiesWorker(
            ILogger<CheckingProxiesWorker> logger,
            IEnumerable<IProxiesChecker> proxiesCheckers,
            Func<Owned<ProxiesDbContext>> proxiesDbContextFactory)
        {
            _logger = logger;
            _proxiesCheckers = proxiesCheckers;
            _proxiesDbContextFactory = proxiesDbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting checking run at: {time}", DateTime.Now);

                    using var dbContext = _proxiesDbContextFactory();
                    var proxiesToCheck = await dbContext.Value.Proxies.ToListAsync(stoppingToken);

                    foreach (var proxyChecker in _proxiesCheckers)
                    {
                        var checkingMethods = await dbContext.Value.CheckingMethods
                            .Where(method => method.Name == proxyChecker.Name)
                            .Where(method => !method.IsDisabled)
                            .ToListAsync(stoppingToken);

                        _logger.LogInformation("{0} proxy checker has {1} checking methods", proxyChecker.Name, checkingMethods.Count());

                        foreach (var checkingMethod in checkingMethods)
                        {
                            try
                            {
                                _logger.LogInformation("Checking proxies using {0}({1}) service", proxyChecker.Name, checkingMethod.Description);
                                var checkingResults = proxyChecker.CheckProxiesAsync(proxiesToCheck, checkingMethod, stoppingToken);
                                var proxiesPassedCount = checkingResults.Where(e => e.Result).Count();
                                _logger.LogInformation("Successfully checked all proxies. Passed count: {0}", proxiesPassedCount);

                                _logger.LogInformation("Adding proxies results to db");
                                await dbContext.Value.CheckingResults.AddRangeAsync(checkingResults, stoppingToken);
                                await dbContext.Value.SaveChangesAsync(stoppingToken);
                                _logger.LogInformation("Successfully added checking results to db");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("Checking proxies using {0}({1}) service failed. \n{2}", proxyChecker.Name, checkingMethod.Description, ex);
                            }
                        }
                    }

                    _logger.LogInformation("Checking run completed. Next run at: {time}", DateTime.Now.Add(RUN_DELAY));
                }
                catch (Exception ex)
                {
                    _logger.LogError("Checking run failed", ex);
                }

                await Task.Delay(RUN_DELAY, stoppingToken);
            }
        }
    }
}