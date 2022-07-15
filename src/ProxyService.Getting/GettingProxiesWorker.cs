using Autofac.Features.OwnedInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProxyService.Core.Models;
using ProxyService.Database;
using ProxyService.Getting.Interfaces;

namespace ProxyService.Getting
{
    public class GettingProxiesWorker : BackgroundService
    {
        private readonly TimeSpan RUN_DELAY = TimeSpan.FromMinutes(30);

        private readonly ILogger<GettingProxiesWorker> _logger;
        private readonly IEnumerable<IProxiesGetter> _proxiesGetters;
        private readonly Func<Owned<ProxiesDbContext>> _proxiesDbContextFactory;

        public GettingProxiesWorker(
            ILogger<GettingProxiesWorker> logger,
            IEnumerable<IProxiesGetter> proxiesGetters,
            Func<Owned<ProxiesDbContext>> proxiesDbContextFactory)
        {
            _logger = logger;
            _proxiesGetters = proxiesGetters;
            _proxiesDbContextFactory = proxiesDbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting getting run at: {time}", DateTime.Now);
                using var dbContext = _proxiesDbContextFactory();
                var activeGettingMethods = await dbContext.Value.GettingMethods.Where(e => !e.IsDisabled).Select(e => e.Name).ToListAsync();

                foreach (var proxyGetter in _proxiesGetters)
                {
                    if (!activeGettingMethods.Contains(proxyGetter.Name))
                    {
                        _logger.LogInformation("{0} proxy getter is disabled. skipping", proxyGetter.Name);
                        continue;
                    }

                    try
                    {
                        _logger.LogInformation("Getting proxies from service {0}", proxyGetter.Name);
                        var newProxies = await proxyGetter.GetProxiesAsync(stoppingToken);
                        _logger.LogInformation("Found {0} proxies from service {1}", newProxies.Count, proxyGetter.Name);

                        _logger.LogInformation("Adding new proxies to db");
                        var existingProxies = await dbContext.Value.Proxies.ToListAsync(stoppingToken);
                        var proxiesToAdd = GetProxiesToAdd(newProxies, existingProxies);
                        await dbContext.Value.Proxies.AddRangeAsync(proxiesToAdd, stoppingToken);
                        await dbContext.Value.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Successfully added {0} new proxies to db. Source: {1}", proxiesToAdd.Count, proxyGetter.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Getting proxies from {0} failed. \n{1}", proxyGetter.Name, ex);
                    }
                }

                _logger.LogInformation("Getting run completed. Next run at: {time}", DateTime.Now.Add(RUN_DELAY));

                await Task.Delay(RUN_DELAY, stoppingToken);
            }
        }

        private List<Proxy> GetProxiesToAdd(IEnumerable<Proxy> newProxies, IEnumerable<Proxy> existingProxies)
        {
            var joinedProxies = from newProxy in newProxies
                                join existingProxy in existingProxies on newProxy.IpPort equals existingProxy.IpPort into gj
                                from subExistingProxy in gj.DefaultIfEmpty()
                                select new
                                {
                                    Proxy = newProxy,
                                    IsNew = subExistingProxy is null,
                                };
            return joinedProxies.Where(e => e.IsNew).Select(e => e.Proxy).ToList();
        }
    }
}