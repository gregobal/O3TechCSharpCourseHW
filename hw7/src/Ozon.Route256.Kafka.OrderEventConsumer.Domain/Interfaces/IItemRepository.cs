using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;

public interface IItemRepository
{
    Task<long> MergeAccountingByStatus(IEnumerable<AccountingByStatus> itemsAccounting, CancellationToken token);

    Task<long> MergeAccountingBySeller(IEnumerable<AccountingBySeller> itemsSales, CancellationToken token);
}
