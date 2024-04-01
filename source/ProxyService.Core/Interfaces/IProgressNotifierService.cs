namespace ProxyService.Core.Interfaces;

public interface IProgressNotifierService : IDisposable
{
    void StartNotifying(string message, int itemsCount, TimeSpan delay);

    T ReportProgress<T>(T result);
}
