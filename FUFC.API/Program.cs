using FUFC.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console() // Writes logs to the console
    .WriteTo.File("logs/api-logs/log-.txt", rollingInterval: RollingInterval.Day) // Logs to daily rolling files
    .CreateLogger();

builder.Host.UseSerilog(); // Attach Serilog to the app

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL DbContext
builder.Services.AddDbContext<UfcContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UfcDB")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Middleware for logging HTTP requests/responses
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();