using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Entities;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure;

public sealed class ItemRepository : IItemRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ItemRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<long> MergeAccountingByStatus(IEnumerable<AccountingByStatus> itemsAccounting, CancellationToken token)
    {
        const string sqlQuery =
            """
            insert into accounting_by_status (item_id, created, delivered, canceled, modified_at)
            select item_id, created, delivered, canceled, modified_at
              from unnest(@Entities)
                on conflict (item_id)
                do update
               set created = accounting_by_status.created + excluded.created
                 , delivered = accounting_by_status.delivered + excluded.delivered
                 , canceled = accounting_by_status.canceled + excluded.canceled
                 , modified_at = excluded.modified_at
            returning id;
            """;

        var aggregateByItemId = new Dictionary<ItemId, AccountingByStatusEntityV1>();
        var now = DateTimeOffset.UtcNow;

        foreach (var (key, created, delivered, canceled) in itemsAccounting)
        {
            if (aggregateByItemId.ContainsKey(key))
            {
                var previous = aggregateByItemId[key];
                aggregateByItemId[key] = previous with
                {
                    Created = previous.Created + created,
                    Delivered = previous.Delivered + delivered,
                    Canceled = previous.Canceled + canceled
                };
            }
            else
            {
                aggregateByItemId.Add(
                    key,
                    new AccountingByStatusEntityV1
                    {
                        ItemId = key.Value,
                        ModifiedAt = now,
                        Created = created,
                        Delivered = delivered,
                        Canceled = canceled
                    });
            }
        }

        var entities = aggregateByItemId.Values.ToArray();

        await using var connection = await _dataSource.OpenConnectionAsync(token);

        var ids = await connection.QueryAsync<long>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    Entities = entities
                },
                cancellationToken: token));

        return ids.ToArray().Length;
    }

    public async Task<long> MergeAccountingBySeller(IEnumerable<AccountingBySeller> itemsSales, CancellationToken token)
    {
        const string sqlQuery =
            """
            insert into accounting_by_seller (seller_id, product_id, currency, balance, quantity, modified_at)
            select seller_id, product_id, currency, balance, quantity, modified_at
              from unnest(@Entities)
                on conflict on constraint accounting_by_seller_seller_id_product_id_currency_key
                do update
               set balance = accounting_by_seller.balance + excluded.balance
                 , quantity = accounting_by_seller.quantity + excluded.quantity
                 , modified_at = excluded.modified_at
            returning id;
            """;

        var aggregateByUniqueKey = new Dictionary<string, AccountingBySellerEntityV1>();
        var now = DateTimeOffset.UtcNow;

        foreach (var (sellerId, productId, currency, price, quantity) in itemsSales)
        {
            var key = $"{sellerId}_{productId}_{currency}";

            if (aggregateByUniqueKey.ContainsKey(key))
            {
                var previous = aggregateByUniqueKey[key];
                aggregateByUniqueKey[key] = previous with
                {
                    Balance = previous.Balance + price * quantity,
                    Quantity = previous.Quantity + quantity
                };
            }
            else
            {
                aggregateByUniqueKey.Add(
                    key,
                    new AccountingBySellerEntityV1
                    {
                        SellerId = sellerId.Value,
                        ProductId = productId.Value,
                        Currency = currency,
                        ModifiedAt = now,
                        Balance = price * quantity,
                        Quantity = quantity
                    });
            }
        }

        var entities = aggregateByUniqueKey.Values.ToArray();

        await using var connection = await _dataSource.OpenConnectionAsync(token);

        var ids = await connection.QueryAsync<long>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    Entities = entities
                },
                cancellationToken: token));

        return ids.ToArray().Length;
    }
}
