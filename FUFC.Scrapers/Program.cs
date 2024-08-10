using System.Reflection;
using FUFC.Scrapers.Common;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.AddConsoleExporter();
    });
});

var logger = loggerFactory.CreateLogger<Program>();

if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
{
    Console.WriteLine("Error: SpiderName is required.");
    Environment.Exit(1);
}

string spiderName = args[0];

Type? spiderType;
try
{
    spiderType = GetSpiderTypeByName(spiderName);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
    Environment.Exit(1);
    return;
}

if (spiderType == null)
{
    Console.WriteLine($"Error: Spider type for '{spiderName}' could not be found.");
    Environment.Exit(1);
}

IUfcSpider? spiderInstance = Activator.CreateInstance(spiderType) as IUfcSpider; 

if (spiderInstance == null)
{
    Console.WriteLine($"Error: Failed to create an instance of spider '{spiderName}'.");
    Environment.Exit(1);
}

Console.WriteLine($"Initiating crawl for {spiderName}...");
 
spiderInstance.Crawl();

static Type GetSpiderTypeByName(string name)
{
    var types = Assembly.GetExecutingAssembly().GetTypes();
    
    var spiderType = types.FirstOrDefault(t =>
        typeof(IUfcSpider).IsAssignableFrom(t) &&
        t.GetProperty("Name")?.GetValue(Activator.CreateInstance(t))?.ToString() == name);

    if (spiderType == null)
    {
        throw new InvalidOperationException($"No spider found with the name '{name}'.");
    }

    return spiderType; 
}
