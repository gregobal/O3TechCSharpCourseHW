using System;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Entities;

public sealed record AccountingBySellerEntityV1
{
    public long Id { get; init; }

    public long SellerId { get; init; }

    public long ProductId { get; init; }

    public required string Currency { get; init; }

    public decimal Balance { get; init; }

    public int Quantity { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }
}
