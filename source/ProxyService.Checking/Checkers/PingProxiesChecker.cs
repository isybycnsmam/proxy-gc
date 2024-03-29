using ProxyService.Core.Models;
using ProxyService.Checking.Interfaces;
using System.Net.NetworkInformation;
using System.Text;

namespace ProxyService.Checking.Ping;

public class PingProxiesChecker : IProxiesChecker
{
    public string Name => "Ping";

    public CheckingResult TestProxy(Proxy? proxy, CheckingMethod _, int checkingSessionId)
    {
        var checkingResult = new CheckingResult()
        {
            ProxyId = proxy?.Id,
            CheckingSessionId = checkingSessionId,
            Result = false,
            ResponseTime = 0,
        };

        try
        {
            const string data = "abcdefghijklmnoprstuwxyz12345678";
            var buffer = Encoding.ASCII.GetBytes(data);
            var options = new PingOptions(64, true);
            var pingSender = new System.Net.NetworkInformation.Ping();

            var ip = proxy?.Ip ?? "localhost";
            var reply = pingSender.Send(ip, timeout: 10000, buffer, options);

            if (reply.Status == IPStatus.Success)
            {
                checkingResult.Result = true;
                checkingResult.ResponseTime = Convert.ToInt32(reply.RoundtripTime);
            }
        }
        catch { }

        return checkingResult;
    }
}
