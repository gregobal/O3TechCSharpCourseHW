using System;
using FluentMigrator;

using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(20231101145300, TransactionBehavior.None)]
public sealed class AddAccountingBySellerV1Type : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) =>
        """
        do $$
            begin
                if not exists (select 1 from pg_type where typname = 'accounting_by_seller_v1') then
                    create type accounting_by_seller_v1 as
                    (
                          id          bigint
                        , seller_id   bigint
                        , product_id  bigint
                        , currency    text
                        , balance     decimal
                        , quantity    integer
                        , modified_at timestamp with time zone
                    );
                end if;
            end
        $$;
        """;

    protected override string GetDownSql(IServiceProvider services) =>
        """
        do $$
            begin
                drop type if exists accounting_by_seller_v1;
            end
        $$;
        """;
}
