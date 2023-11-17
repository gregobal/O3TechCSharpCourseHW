using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _repository;

    public ItemService(IItemRepository repository) => _repository = repository;

    public async Task<long> AccountingByStatus(IEnumerable<OrderEvent> orderEvents, CancellationToken token)
    {
        var orderEventsList = orderEvents.ToList();

        if (orderEventsList.Count == 0)
        {
            return 0;
        }

        var itemsAccounting = orderEventsList.SelectMany(
            o =>
                o.Positions.Select(p => GetAccountingFromOrderPositionByStatus(p, o.Status)));

        var result = await _repository.MergeAccountingByStatus(itemsAccounting, token);

        return result;
    }

    public async Task<long> AccountingBySeller(IEnumerable<OrderEvent> orderEvents, CancellationToken token)
    {
        var orderEventsDelivered = orderEvents.Where(x => x.Status == Status.Delivered)
            .ToList();

        if (orderEventsDelivered.Count == 0)
        {
            return 0;
        }

        const int itemIdSplitter = 1_000_000;

        var itemsSales = orderEventsDelivered.SelectMany(
            o => o.Positions.Select(
                p =>
                    new AccountingBySeller(
                        new SellerId(p.ItemId.Value / itemIdSplitter),
                        new ProductId(p.ItemId.Value % itemIdSplitter),
                        p.Price.Currency,
                        p.Price.Value,
                        p.Quantity)));

        var result = await _repository.MergeAccountingBySeller(itemsSales, token);

        return result;
    }

    private AccountingByStatus GetAccountingFromOrderPositionByStatus(OrderEventPosition position, Status status)
        => status switch
        {
            Status.Created => new AccountingByStatus(position.ItemId, position.Quantity, 0, 0),
            Status.Delivered => new AccountingByStatus(position.ItemId, 0, position.Quantity, 0),
            Status.Canceled => new AccountingByStatus(position.ItemId, 0, 0, position.Quantity),
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
}
