namespace ProxyService.Core.Interfaces;

public interface IProgressNotifierService : IDisposable
{
    public void StartNotifying(string message, int itemsCount, TimeSpan delay);

    public T ReportProgress<T>(T result);
}
