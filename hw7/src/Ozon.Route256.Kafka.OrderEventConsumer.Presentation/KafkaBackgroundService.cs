using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka.Interfaces;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation;

public class KafkaBackgroundService : BackgroundService
{
    private readonly IAsyncConsumer _consumer;
    private readonly ILogger<KafkaBackgroundService> _logger;

    public KafkaBackgroundService(
        ILogger<KafkaBackgroundService> logger,
        IAsyncConsumer consumer)
    {
        _logger = logger;
        _consumer = consumer;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        (_consumer as IDisposable)?.Dispose();

        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _consumer.Consume(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");
        }
    }
}
