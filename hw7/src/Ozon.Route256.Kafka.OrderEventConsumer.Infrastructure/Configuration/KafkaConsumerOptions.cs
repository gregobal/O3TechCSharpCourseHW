namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Configuration;

public record KafkaConsumerOptions
{
    public required string BootstrapServers { get; init; }

    public required string GroupId { get; init; }

    public required string Topic { get; init; }

    public int ChannelCapacity { get; init; }

    public int BufferDelaySec { get; init; }

    public int RetryWhenException { get; init; }
}
