using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;

public sealed record OrderEventPosition(ItemId ItemId, int Quantity, Money Price);
