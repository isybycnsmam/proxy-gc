namespace ProxyService.Core.Models
{
    public sealed class CheckingResult
    {
        public int Id { get; set; }

        public bool Result { get; set; }
        public int ResponseTime { get; set; }
        public bool Ignore { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public int ProxyId { get; set; }
        public Proxy Proxy { get; set; }

        public int CheckingMethodSessionsId { get; set; }
        public CheckingMethodSession CheckingMethodSessions { get; set; }
    }
}
