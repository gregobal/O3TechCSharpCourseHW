using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

configuration["Logging:LogLevel:Default"] = "Information";
configuration["CsvDataFileNames:MonthFactor"] = "/Assets/month_factor.csv";
configuration["CsvDataFileNames:SaleHistory"] = "/Assets/sale_history.csv";

var services = new ServiceCollection();

services.AddScoped<IConfiguration>(_ => configuration);
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddConfiguration(configuration.GetSection("Logging"));
});

services.AddScoped<ISaleHistoryRepository, SaleHistoryRepository>();
services.AddScoped<IMonthFactorRepository, MonthFactorRepository>();
services.AddScoped<ISaleAnalyticsService, SaleAnalyticsService>();

using var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var saleAnalyticsService = serviceProvider.GetRequiredService<ISaleAnalyticsService>();

var idArg = new Argument<int>("id", "Product Id");
var daysArg = new Argument<int>("days", "Days count");
var supplyDateArg = new Argument<DateOnly?>(name: "supply", () => null, "Supply date");

var ads = new Command("ads", "Calculate average daily sales")
{
    idArg
};
ads.SetHandler(
    id => Console.WriteLine(saleAnalyticsService.CalculateAds(id)),
    idArg
);

var prediction = new Command("prediction", "Calculate sales prediction")
{
    idArg, daysArg
};

prediction.SetHandler(
    (id, days) => Console.WriteLine(saleAnalyticsService.CalculateSalesPrediction(id, days)),
    idArg, daysArg
);

var demand = new Command("demand", "Calculate purchasing requirement")
{
    idArg, daysArg, supplyDateArg
};

demand.SetHandler(
    (id, days, supply) => Console.WriteLine(saleAnalyticsService.CalculateDemand(id, days, supply)),
    idArg, daysArg, supplyDateArg
);

var root = new RootCommand("CLI app for sale analytics service")
{
    ads, prediction, demand
};

var parser = new CommandLineBuilder(root)
    .UseDefaults()
    .UseHelp()
    .UseExceptionHandler((ex, ctx) =>
    {
        switch (ex)
        {
            case DomainException:
                Console.WriteLine($"Calculation error: {ex.Message}");
                break;
            default:
                Console.WriteLine($"Program ended unexpectedly: {ex.Message}");
                break;
        }

        logger.LogDebug("{}", ex);
        ctx.ExitCode = 1;
    })
    .Build();

parser.Invoke(args);