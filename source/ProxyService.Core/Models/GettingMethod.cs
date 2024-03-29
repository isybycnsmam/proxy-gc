namespace ProxyService.Core.Models;

public sealed class GettingMethod
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDisabled { get; set; }
}
