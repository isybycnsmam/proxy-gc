using Microsoft.Extensions.Logging;
using ProxyService.Core.Interfaces;
using ProxyService.Database.Repositories;
using ProxyService.Getting.Interfaces;

namespace ProxyService.Getting;

public class GettingProxiesProcedure(
    ILogger<GettingProxiesProcedure> logger,
    IEnumerable<IProxiesGetter> proxiesGetters,
    ProxiesRepository proxiesRepository,
    GettingMethodsRepository gettingMethodsRepository) : IProcedure
{
    public string Name => "Getting";
    private readonly ILogger<GettingProxiesProcedure> _logger = logger;
    private readonly IEnumerable<IProxiesGetter> _proxiesGetters = proxiesGetters;
    private readonly ProxiesRepository _proxiesRepository = proxiesRepository;
    private readonly GettingMethodsRepository _gettingMethodsRepository = gettingMethodsRepository;

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Getting active getting methods");
        var activeGettingMethods = await _gettingMethodsRepository.GetActiveGettingMethodsNames(stoppingToken);
        
        foreach (var proxyGetter in _proxiesGetters)
        {
            if (!activeGettingMethods.Contains(proxyGetter.Name))
            {
                _logger.LogInformation("{proxyGetterName} proxy getter is disabled. skipping", proxyGetter.Name);
                continue;
            }

            try
            {
                _logger.LogInformation("Getting proxies from service {proxyGetterName}", proxyGetter.Name);
                var newProxies = await proxyGetter.GetProxiesAsync(stoppingToken);
                _logger.LogInformation("Found {newProxiesCount} proxies from service {proxyGetterName}", newProxies.Count, proxyGetter.Name);

                _logger.LogInformation("Adding new proxies to db");
                var newProxiesCount = await _proxiesRepository.InsertNewProxies(newProxies, stoppingToken);
                _logger.LogInformation("Successfully added {newProxiesCount} new proxies to db. Source: {proxyGetterName}", newProxiesCount, proxyGetter.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError("Getting proxies from {proxyGetterName} failed. \n{ex}", proxyGetter.Name, ex);
            }
        }
    }
}