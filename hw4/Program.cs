using homework_4;
using homework_4.Configuration;
using homework_4.Interfaces;
using homework_4.Repositories;
using homework_4.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .Build();

var services = new ServiceCollection();

services.AddScoped<IProductAnalyticsRepository, ProductAnalyticsCsvRepository>();
services.AddScoped<IProductDemandRepository, ProductDemandCsvRepository>();
services.AddScoped<IProductAnalyticService, ProductAnalyticService>();
services.AddScoped<ICalculateService, CalculateService>();

services.AddSingleton<App>();

services.Configure<AppOptions>(configuration.GetSection("App"));
services.Configure<RepositoryOptions>(configuration.GetSection("Repository"));

services.AddLogging(builder => builder.AddConfiguration(configuration.GetSection("Logging")).AddConsole());

var provider = services.BuildServiceProvider();
var app = provider.GetRequiredService<App>();

await app.RunAsync();