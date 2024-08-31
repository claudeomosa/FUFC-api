using FUFC.Scrapers.Common;
using FUFC.Scrapers.Spiders;
using FUFC.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = new ConfigurationBuilder();

BuildConfig(builder);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File($"logs/log-{DateTime.Now:yyyy-MM-dd}.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();


var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<IUfcSpider, UfcStatsSpider>();
        services.AddTransient<IUfcSpider, UfcOfficialSpider>();
        services.AddDbContext<UfcContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("UfcDB")));

    })
    .UseSerilog()
    .Build();
/*
if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
{
    Console.WriteLine("Error: SpiderName is required.");
    Environment.Exit(1);
}
*/
string spiderName = "UFC Stats Spider";

if (spiderName == "UFC Stats Spider")
{
    var svc = ActivatorUtilities.CreateInstance<UfcStatsSpider>(host.Services);
    svc.Crawl();
}
else if (spiderName == "UFC Official Spider")
{
    var svc = ActivatorUtilities.CreateInstance<UfcOfficialSpider>(host.Services);
    svc.Crawl();
}


static void BuildConfig(IConfigurationBuilder configurationBuilder)
{
    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
}