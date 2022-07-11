namespace ProxyService.Core.Models
{
    public sealed class CheckingMethod
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string TestTarget { get; set; }
        public string Description { get; set; }
        public bool IsDisabled { get; set; }

        public List<CheckingResult> CheckingResults { get; set; }
    }
}
