namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;

public readonly record struct Money(decimal Value, string Currency);
