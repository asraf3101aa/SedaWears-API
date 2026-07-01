using System.CommandLine;
using SedaWears.Application;
using SedaWears.Infrastructure;
using SedaWears.Presentation;
using SedaWears.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddApiServices();

var app = builder.Build();

// Apply migrations at startup
await app.InitialiseDatabaseAsync();

// CLI Commands
var rootCommand = new RootCommand("SedaWears Management CLI");
rootCommand.SetupCliCommands(app);

if (args.Length > 0 && (args[0] == "create-admin" || args[0] == "seed-roles" || args[0] == "-h" || args[0] == "--help"))
{
    await rootCommand.InvokeAsync(args);
    return;
}

// Configure Pipeline
app.UseApplicationPipeline();

app.Run();