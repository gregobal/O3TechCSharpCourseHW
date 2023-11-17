using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.NameTranslation;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Configuration;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka.Interfaces;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentMigrator(
        this IServiceCollection services,
        string connectionString,
        Assembly assembly)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(
                builder => builder
                    .AddPostgres()
                    .ScanIn(assembly).For.Migrations())
            .AddOptions<ProcessorOptions>()
            .Configure(
                options =>
                {
                    options.ProviderSwitches = "Force Quote=false";
                    options.Timeout = TimeSpan.FromMinutes(10);
                    options.ConnectionString = connectionString;
                });

        return services;
    }

    public static IServiceCollection AddDataSource(
        this IServiceCollection services,
        string connectionString)
    {
        var translator = new NpgsqlSnakeCaseNameTranslator();

        services.AddNpgsqlDataSource(
            connectionString,
            builder =>
            {
                builder.MapComposite<AccountingByStatusEntityV1>("accounting_by_status_v1", translator);
                builder.MapComposite<AccountingBySellerEntityV1>("accounting_by_seller_v1", translator);
            });

        return services;
    }

    public static IServiceCollection AddKafkaHandler<TKey, TValue, THandler>(this IServiceCollection services, IConfiguration configuration)
        where THandler : class, IHandler<TKey, TValue>
    {
        services.Configure<KafkaConsumerOptions>(configuration.GetSection(nameof(KafkaConsumerOptions)));

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        services
            .AddSingleton<IHandler<TKey, TValue>, THandler>()
            .AddSingleton<IDeserializer<TKey>>(_ => null!)
            .AddSingleton<IDeserializer<TValue>>(_ => new SystemTextJsonSerializer<TValue>(jsonSerializerOptions))
            .AddSingleton<IAsyncConsumer, KafkaAsyncConsumer<TKey, TValue>>();

        return services;
    }
}
