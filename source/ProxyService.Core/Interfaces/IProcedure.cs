namespace ProxyService.Core.Interfaces;

public interface IProcedure
{
    string Name { get; }

    Task ExecuteAsync(CancellationToken stoppingToken);
}
