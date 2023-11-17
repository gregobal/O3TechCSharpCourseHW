using System;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Entities;

public sealed record AccountingByStatusEntityV1
{
    public long Id { get; init; }

    public long ItemId { get; init; }

    public int Created { get; init; }

    public int Delivered { get; init; }

    public int Canceled { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }
}
