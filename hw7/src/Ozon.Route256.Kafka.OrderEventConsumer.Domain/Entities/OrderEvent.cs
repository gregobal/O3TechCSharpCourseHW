using System;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;

public sealed record OrderEvent(
    OrderId OrderId,
    UserId UserId,
    WarehouseId WarehouseId,
    Status Status,
    DateTime Moment,
    OrderEventPosition[] Positions);
