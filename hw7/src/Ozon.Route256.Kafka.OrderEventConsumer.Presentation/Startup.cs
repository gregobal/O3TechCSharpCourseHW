using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Dtos;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Services;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Profiles;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration) => _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddLogging();

        var connectionString = _configuration.GetConnectionString("Postgres")!;
        services
            .AddFluentMigrator(connectionString, typeof(SqlMigration).Assembly)
            .AddDataSource(connectionString)
            .AddSingleton<IItemRepository, ItemRepository>();

        services
            .AddAutoMapper(typeof(AutoMapperProfile))
            .AddSingleton<IItemService, ItemService>()
            .AddKafkaHandler<Ignore, OrderEventDto, ItemHandler>(_configuration)
            .AddHostedService<KafkaBackgroundService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
}
