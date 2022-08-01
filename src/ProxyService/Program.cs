using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProxyService.Database;
using ProxyService.Configs;
using ProxyService.Getting;
using ProxyService.Getting.TextSpysOne;
using ProxyService.Getting.Interfaces;
using ProxyService.Checking.Interfaces;
using ProxyService.Checking.Ping;
using ProxyService.Checking;
using ProxyService.Core.Services;
using ProxyService.Checking.Site;
using ProxyService.Getting.ProxyOrg;
using ProxyService.Getting.HttpsSpysOne;

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

         var dbConfigUration = hostContext.Configuration.GetSection("Database");
        services.AddDbContext<ProxiesDbContext>(options =>
            options.UseMySql(
                dbConfigUration["ConnectionString"],
                new MySqlServerVersion(dbConfigUration["ServerVersion"])));

        services.AddHostedService<GettingProxiesWorker>();
        services.AddHostedService<CheckingProxiesWorker>();
    })
    .Build();

await host.RunAsync();
