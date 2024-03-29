using Microsoft.Extensions.Logging;
using ProxyService.Checking.Exceptions;
using ProxyService.Checking.Interfaces;
using ProxyService.Core.Interfaces;
using ProxyService.Core.Models;
using ProxyService.Database.Repositories;
using System.Diagnostics;

namespace ProxyService.Checking;

public sealed class CheckingProxiesProcedure(
    ILogger<CheckingProxiesProcedure> logger,
    IEnumerable<IProxiesChecker> proxiesCheckers,
    Func<IProgressNotifierService> progressNotifierServiceFactory,
    ProxiesRepository proxiesRepository,
    CheckingRunsRepository checkingRunsRepository,
    CheckingResultsRepository checkingResultsRepository,
    CheckingSessionsRepository checkingMethodSessionsRepository,
    CheckingMethodsRepository checkingMethodsRepository) : IProcedure
{
    public string Name => "Checking";
    private readonly ILogger<CheckingProxiesProcedure> _logger = logger;
    private readonly IEnumerable<IProxiesChecker> _proxiesCheckers = proxiesCheckers;
    private readonly Func<IProgressNotifierService> _progressNotifierServiceFactory = progressNotifierServiceFactory;
    private readonly ProxiesRepository _proxiesRepository = proxiesRepository;
    private readonly CheckingRunsRepository _checkingRunsRepository = checkingRunsRepository;
    private readonly CheckingResultsRepository _checkingResultsRepository = checkingResultsRepository;
    private readonly CheckingSessionsRepository _checkingMethodSessionsRepository = checkingMethodSessionsRepository;
    private readonly CheckingMethodsRepository _checkingMethodsRepository = checkingMethodsRepository;

    private const int CONNECTION_TESTING_INTERVAL = 60000;// TODO: move to configuration
    private const int NOTIFYING_DELAY = 10000;// TODO: move to configuration
    private const int CHECKING_DEGREE_OF_PARALLELISM = 5;// TODO: move to configuration

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting proxies to check");
        var proxiesToCheck = await _proxiesRepository.GetProxiesToCheck(cancellationToken);
        if (proxiesToCheck.Count == 0)
            throw new NoProxiesFoundException();
        _logger.LogInformation("Found {proxiesToCheckCount} proxies to check", proxiesToCheck.Count);

        _logger.LogInformation("Adding checking run entry");
        var checkingRun = await _checkingRunsRepository.CreateCheckingRun(cancellationToken);
        _logger.LogInformation("Successfully added checking run entry id: {id}", checkingRun.Id);

        var proxyCheckersWithMethods = await GetProxyCheckersWithTheirCheckingMethods(cancellationToken);
        var proxyCheckersWithMethodsCount = proxyCheckersWithMethods.Count;
        _logger.LogInformation("Found {checkingMethodsCount} checking methods", proxyCheckersWithMethods.Count);

        var currentMethodIndex = 0;
        foreach (var (checker, checkingMethod) in proxyCheckersWithMethods)
        {
            _logger.LogInformation("Starting checking session using {checkerName}({description}) service. {methodIndex} out of {methodsCount}",
                checker.Name,
                checkingMethod.Description,
                ++currentMethodIndex,
                proxyCheckersWithMethodsCount);

            CancellationTokenSource? connectionTestingCts = null;

            try
            {
                _logger.LogInformation("Adding checking session entry");
                var checkingSession = await _checkingMethodSessionsRepository.CreateCheckingSession(checkingRun.Id, checkingMethod, cancellationToken);
                _logger.LogInformation("Successfully added checking session entry id: {id}", checkingSession.Id);

                _logger.LogInformation("Starting control testing without proxy");
                connectionTestingCts = StartConnectionTestingWithoutProxy(checkingSession.Id, checker, checkingMethod, cancellationToken);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _logger.LogInformation("Checking proxies");
                var checkingResults = CheckProxies(checker, proxiesToCheck, checkingSession.Id, checkingMethod, cancellationToken);
                stopwatch.Stop();

                var proxiesPassedCount = checkingResults.Count(e => e.Result);
                var passRate = Math.Round((double)proxiesPassedCount / proxiesToCheck.Count * 100);
                _logger.LogInformation("Successfully checked all proxies. Elapsed: {elapsedMs}. Pass count: {proxiesPassedCount}. Pass rate: {passRate}%", stopwatch.ElapsedMilliseconds, proxiesPassedCount, passRate);

                _logger.LogInformation("Adding proxies results to db");
                await _checkingResultsRepository.InsertCheckingResults(checkingResults, cancellationToken);
                _logger.LogInformation("Successfully added checking results to db");

                await _checkingMethodSessionsRepository.UpdateElapsed(checkingSession.Id, stopwatch.ElapsedMilliseconds, cancellationToken);
                _logger.LogInformation("Successfully updated checking session elapsed time");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checking proxies using {checkerName}({description}) service failed.",
                    checker.Name,
                    checkingMethod.Description);
            }
            finally
            {
                connectionTestingCts?.Cancel();
                connectionTestingCts?.Dispose();
                _logger.LogInformation("Checking session ended.");
            }
        }
    }

    private async Task<List<(IProxiesChecker checker, CheckingMethod method)>> GetProxyCheckersWithTheirCheckingMethods(CancellationToken cancellationToken)
    {
        var checkingMethods = await _checkingMethodsRepository.GetActiveCheckingMethods(cancellationToken);

        var result = new List<(IProxiesChecker checker, CheckingMethod method)>();

        foreach (var proxyChecker in _proxiesCheckers)
        {
            var proxyCheckerMethods = checkingMethods
                .Where(e => e.Name == proxyChecker.Name)
                .ToList();

            _logger.LogInformation("{proxyCheckerName} proxy checker has {proxyCheckerMethodsCount} checking methods", proxyChecker.Name, proxyCheckerMethods.Count);

            result.AddRange(proxyCheckerMethods.Select(method => (proxyChecker, method)));
        }

        return result;
    }

    private List<CheckingResult> CheckProxies(
        IProxiesChecker checker,
        List<Proxy> proxiesToCheck,
        int checkingSessionId,
        CheckingMethod checkingMethod,
        CancellationToken cancellationToken)
    {
        using var progressNotifierService = _progressNotifierServiceFactory();

        progressNotifierService.StartNotifying(
                    message: $"Checking proxies {checker.Name}({checkingMethod.Description}) progress: {{progress}}%",
                    itemsCount: proxiesToCheck.Count,
                    delay: TimeSpan.FromMilliseconds(NOTIFYING_DELAY));

        var checkingResults = proxiesToCheck
            .AsParallel()
            .WithDegreeOfParallelism(CHECKING_DEGREE_OF_PARALLELISM)
            .WithCancellation(cancellationToken)
            .Select(proxy => checker.TestProxy(proxy, checkingMethod, checkingSessionId))
            .Select(progressNotifierService.ReportProgress)
            .ToList();

        return checkingResults;
    }

    private CancellationTokenSource StartConnectionTestingWithoutProxy(
        int checkingMethodSessionId,
        IProxiesChecker proxyChecker,
        CheckingMethod checkingMethod,
        CancellationToken cancellationToken)
    {
        var internalTokenSource = new CancellationTokenSource();

        Task.Run(async () =>
        {
            var linkedStoppingCts = CancellationTokenSource.CreateLinkedTokenSource(internalTokenSource.Token, cancellationToken);
            var token = linkedStoppingCts.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    _logger.LogInformation("Testing connection without proxy.");
                    var connectionTestingResult = proxyChecker.TestProxy(null, checkingMethod, checkingMethodSessionId);
                    _logger.LogInformation("Connection testing result: {result}", connectionTestingResult.Result ? "Success" : "Failed");
                    await _checkingResultsRepository.InsertCheckingResults([connectionTestingResult], token);
                    await Task.Delay(CONNECTION_TESTING_INTERVAL, token);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Connection testing without proxy for {checkerName}({description}) has been stopped.",
                                proxyChecker.Name,
                                checkingMethod.Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection testing failed.");
            }
            
            linkedStoppingCts.Dispose();
        }, cancellationToken);

        return internalTokenSource;
    }
}
