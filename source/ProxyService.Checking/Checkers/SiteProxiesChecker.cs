using ProxyService.Core.Models;
using ProxyService.Checking.Interfaces;
using System.Net;
using System.Diagnostics;

namespace ProxyService.Checking.Site;

public class SiteProxiesChecker : IProxiesChecker
{
    public string Name => "Site";

    // TODO: Update obsolete method
    public CheckingResult TestProxy(Proxy proxy, CheckingMethod checkingMethod, int checkingSessionId)
    {
        if (string.IsNullOrEmpty(checkingMethod.TestTarget))
            throw new InvalidOperationException("Invalid configuration of checking method id: {checkingMethod.Id}. TestTarget is null or empty");

        var checkingResult = new CheckingResult()
        {
            ProxyId = proxy?.Id,
            CheckingSessionId = checkingSessionId,
            Result = false,
            ResponseTime = 0,
        };

        try
        {
            var request = (HttpWebRequest)WebRequest.Create(checkingMethod.TestTarget);
            request.Timeout = 10000;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";

            if (proxy is not null)
                request.Proxy = new WebProxy(proxy.Ip, proxy.Port);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using var response = request.GetResponse();
            stopwatch.Stop();

            checkingResult.Result = true;
            checkingResult.ResponseTime = (int)stopwatch.ElapsedMilliseconds;
        }
        catch { }

        return checkingResult;
    }
}