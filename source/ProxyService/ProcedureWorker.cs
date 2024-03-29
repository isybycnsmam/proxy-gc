using ProxyService.Checking.Exceptions;
using ProxyService.Core.Interfaces;

namespace ProxyService;

public class ProcedureWorker<TProcedure>(
    ILogger<ProcedureWorker<TProcedure>> logger,
    Func<TProcedure> procedure) : BackgroundService
        where TProcedure : IProcedure
{
    private readonly TimeSpan _runDelay = TimeSpan.FromMinutes(30);// TODO: move to configuration
    private readonly TimeSpan _noProxiesDelay = TimeSpan.FromMinutes(2);// TODO: move to configuration

    private readonly ILogger<ProcedureWorker<TProcedure>> _logger = logger;
    private readonly Func<TProcedure> _procedure = procedure;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                TProcedure procedure = _procedure();

                _logger.LogInformation("Starting {name} procedure at: {time}", procedure.Name, DateTime.Now);

                await procedure.ExecuteAsync(stoppingToken);

                _logger.LogInformation("{name} procedure completed. Next run at: {time}", procedure.Name, DateTime.Now.Add(_runDelay));
            }
            catch (NoProxiesFoundException)
            {
                _logger.LogInformation("No proxies to check, retrying in 2 minutes");
                await Task.Delay(_noProxiesDelay, stoppingToken);
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Run failed");
            }

            await Task.Delay(_runDelay, stoppingToken);
        }
    }
}