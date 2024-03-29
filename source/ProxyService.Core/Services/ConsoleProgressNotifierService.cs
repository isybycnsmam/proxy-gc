using Microsoft.Extensions.Logging;
using ProxyService.Core.Interfaces;

namespace ProxyService.Core.Services;

public sealed class ConsoleProgressNotifierService(ILogger<ConsoleProgressNotifierService> logger) : IProgressNotifierService
{
    private readonly ILogger<ConsoleProgressNotifierService> _logger = logger;

    private CancellationTokenSource _cts;
    private bool _isStartedTask;
    private int _itemsCount;
    private int _completeCount;
    private float _progress;
    private bool _disposed;

    public void StartNotifying(string message, int itemsCount, TimeSpan delay)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_isStartedTask)
            throw new InvalidOperationException("Notifying task has already been started");

        _cts = new CancellationTokenSource();
        _itemsCount = itemsCount;
        _completeCount = 0;
        _progress = 0;
        _isStartedTask = true;

        var ct = _cts.Token;
        Task.Run(async () =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    _logger.LogInformation(message, Math.Round(_progress));
                    await Task.Delay(delay, ct);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Progress notifier task has been stopped");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in progress notifier task");
            }
        });
    }

    public T ReportProgress<T>(T result)
    {
        _progress = Interlocked.Increment(ref _completeCount) / (float)_itemsCount * 100;
        return result;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        _cts = null;
        _disposed = true;
    }
}
