using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;

public interface IItemService
{
    Task<long> AccountingByStatus(IEnumerable<OrderEvent> orderEvents, CancellationToken token);

    Task<long> AccountingBySeller(IEnumerable<OrderEvent> orderEvents, CancellationToken token);
}
