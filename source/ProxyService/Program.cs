using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ProxyService;
using ProxyService.Checking;
using ProxyService.Checking.Interfaces;
using ProxyService.Checking.Ping;
using ProxyService.Checking.Site;
using ProxyService.Core.Interfaces;
using ProxyService.Core.Services;
using ProxyService.Database;
using ProxyService.Database.Repositories;
using ProxyService.Getting;
using ProxyService.Getting.HttpsSpysOne;
using ProxyService.Getting.Interfaces;
using ProxyService.Getting.ProxyOrg;
using ProxyService.Getting.TextSpysOne;

var host = Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.RegisterType<TextSpysOneProxiesGetter>().As<IProxiesGetter>();
        builder.RegisterType<ProxyOrgProxiesGetter>().As<IProxiesGetter>();
        builder.RegisterType<HttpsSpysOneProxiesGetter>().As<IProxiesGetter>();

        builder.RegisterType<PingProxiesChecker>().As<IProxiesChecker>();
        builder.RegisterType<SiteProxiesChecker>().As<IProxiesChecker>();

        builder.RegisterType<ConsoleProgressNotifierService>().As<IProgressNotifierService>();

        builder.RegisterType<CheckingResultsRepository>();
        builder.RegisterType<CheckingRunsRepository>();
        builder.RegisterType<CheckingSessionsRepository>();
        builder.RegisterType<CheckingMethodsRepository>();
        builder.RegisterType<GettingMethodsRepository>();
        builder.RegisterType<ProxiesRepository>();

        builder.RegisterType<CheckingProxiesProcedure>();
        builder.RegisterType<GettingProxiesProcedure>();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient();

        var dbConfiguration = hostContext.Configuration.GetSection("Database");
        services.AddDbContext<ProxiesDbContext>(options =>
            options.UseMySql(
                dbConfiguration["ConnectionString"],
                new MySqlServerVersion(dbConfiguration["ServerVersion"])),
            ServiceLifetime.Transient);

        services.AddHostedService<ProcedureWorker<CheckingProxiesProcedure>>();
        services.AddHostedService<ProcedureWorker<GettingProxiesProcedure>>();
    })
    .Build();

await host.RunAsync();
