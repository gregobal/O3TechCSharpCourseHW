using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;

public sealed record AccountingByStatus(
    ItemId ItemId,
    int Created,
    int Delivered,
    int Canceled);
