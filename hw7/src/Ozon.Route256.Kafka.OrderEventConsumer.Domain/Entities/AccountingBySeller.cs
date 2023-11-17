using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;

public record AccountingBySeller(
    SellerId SellerId,
    ProductId ProductId,
    string Currency,
    decimal Price,
    int Quantity);
