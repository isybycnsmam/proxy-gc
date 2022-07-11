using ProxyService.Core.Enums;

namespace ProxyService.Core.Models
{
    public sealed class Proxy
    {
        public int Id { get; set; }

        public string Ip { get; set; }
        public int Port { get; set; }
        public string IpPort => $"{Ip}:{Port}";

        public string CountryCode { get; set; }
        public ProxyType Type { get; set; }
        public ProxyAnonymity Anonymity { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public List<CheckingResult> CheckingResults { get; set; }
    }
}
