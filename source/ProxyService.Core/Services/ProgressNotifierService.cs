using Microsoft.Extensions.Logging;

namespace ProxyService.Core.Services
{
    public class ProgressNotifierService
    {
        public readonly ILogger<ProgressNotifierService> _logger;
        private CancellationTokenSource _cts;
        private int _completeCount;
        private int _itemsCount;

        public float Progress { get; private set; } = 0;

        public ProgressNotifierService(
            ILogger<ProgressNotifierService> logger)
        {
            _logger = logger;
        }

        public void StartNotifying(string message, int itemsCount, TimeSpan delay)
        {
            _itemsCount = itemsCount;
            _cts = new CancellationTokenSource();
            _completeCount = 0;
            Progress = 0;

            var ct = _cts.Token;
            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    _logger.LogInformation(message, Math.Round(Progress));
                    await Task.Delay(delay, ct);
                }
            }).ContinueWith(_ => _cts.Dispose());
        }

        public void StopNotifying()
        {
            _cts.Cancel();
        }

        public T ReportProgress<T>(T result)
        {
            Progress = Interlocked.Increment(ref _completeCount) / (float)_itemsCount * 100;
            return result;
        }
    }
}
