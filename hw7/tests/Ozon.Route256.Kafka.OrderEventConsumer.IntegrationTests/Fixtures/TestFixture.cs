using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Kafka.OrderEventConsumer.IntegrationTests.Fixtures;

public class TestFixture
{
    private const string ConnectionString =
        "User ID=postgres;Password=pwd;Host=localhost;Port=5432;Database=route256;Pooling=true;";

    public TestFixture()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(
                services =>
                {
                    services.AddFluentMigrator(ConnectionString, typeof(SqlMigration).Assembly);
                    services.AddDataSource(ConnectionString);
                    services.AddScoped<IItemRepository, ItemRepository>();
                })
            .Build();

        using var scope = host.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateDown(0);
        runner.MigrateUp();

        var serviceProvider = host.Services;
        ItemRepository = serviceProvider.GetRequiredService<IItemRepository>();
    }

    public IItemRepository ItemRepository { get; }
}
