using System;
using FluentMigrator;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(20231101145100, TransactionBehavior.None)]
public sealed class InitSchema : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) =>
        """
        create table accounting_by_status
        (
            id          bigserial
                primary key,
            item_id     bigint  not null
                unique,
            created     integer not null,
            delivered   integer not null,
            canceled    integer not null,
            modified_at timestamp with time zone
        );

        create table accounting_by_seller
        (
            id          bigserial
                primary key,
            seller_id   bigint  not null,
            product_id  bigint  not null,
            currency    text    not null,
            balance     decimal not null,
            quantity    integer not null,
            modified_at timestamp with time zone,
            unique (seller_id, product_id, currency)
        );
        """;

    protected override string GetDownSql(IServiceProvider services) =>
        """
        drop table accounting_by_status;

        drop table accounting_by_seller;
        """;
}
