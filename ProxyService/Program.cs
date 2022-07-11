using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProxyService.Database;
using ProxyService.Configs;
using ProxyService.Getting;
using ProxyService.Getting.SpysOne;
using ProxyService.Getting.Interfaces;
using ProxyService.Checking.Interfaces;
using ProxyService.Checking.Ping;
using ProxyService.Checking;
using ProxyService.Core.Services;
using ProxyService.Checking.Site;
using ProxyService.Getting.ProxyOrg;

var config = JsonConvert.DeserializeObject<Config>(
    File.ReadAllText("appsettings.json"));

var host = Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.RegisterType<SpysOneProxiesGetter>().As<IProxiesGetter>();
        builder.RegisterType<ProxyOrgProxiesGetter>().As<IProxiesGetter>();

        builder.RegisterType<PingProxiesChecker>().As<IProxiesChecker>();
        builder.RegisterType<SiteProxiesChecker>().As<IProxiesChecker>();

        builder.RegisterType<ProgressNotifierService>();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient();

        services.AddDbContext<ProxiesDbContext>(options => 
            options.UseMySql(
                config.Database.ConnectionString, 
                new MySqlServerVersion(config.Database.ServerVersion)));

        services.AddHostedService<GettingProxiesWorker>();
        services.AddHostedService<CheckingProxiesWorker>();
    })
    .Build();

await host.RunAsync();
