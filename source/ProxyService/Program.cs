using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ProxyService.Checking;
using ProxyService.Checking.Interfaces;
using ProxyService.Checking.Ping;
using ProxyService.Checking.Site;
using ProxyService.Core.Services;
using ProxyService.Database;
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

        builder.RegisterType<ProgressNotifierService>();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient();

        var dbConfiguration = hostContext.Configuration.GetSection("Database");
        services.AddDbContext<ProxiesDbContext>(options =>
            options.UseMySql(
                dbConfiguration["ConnectionString"],
                new MySqlServerVersion(dbConfiguration["ServerVersion"])));

        services.AddHostedService<GettingProxiesWorker>();
        services.AddHostedService<CheckingProxiesWorker>();
    })
    .Build();

await host.RunAsync();
