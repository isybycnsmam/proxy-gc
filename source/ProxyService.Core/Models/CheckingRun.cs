namespace ProxyService.Core.Models;

public sealed class CheckingRun
{
    public int Id { get; set; }
    public bool Ignore { get; set; }
    public string Description { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;

    public List<CheckingSession> CheckingSessions { get; set; }
}
