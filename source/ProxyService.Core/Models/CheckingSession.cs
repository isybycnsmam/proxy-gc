namespace ProxyService.Core.Models;

public sealed class CheckingSession
{
    public int Id { get; set; }

    public bool Ignore { get; set; }
    public int Elapsed { get; set; }
    public string Description { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;

    public int CheckingMethodId { get; set; }
    public CheckingMethod CheckingMethod { get; set; }

    public int CheckingRunId { get; set; }
    public CheckingRun CheckingRun { get; set; }

    public List<CheckingResult> CheckingResults { get; set; }
}
