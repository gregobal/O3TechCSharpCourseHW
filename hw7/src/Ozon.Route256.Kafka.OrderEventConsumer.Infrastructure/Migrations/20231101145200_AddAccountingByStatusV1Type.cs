using System;
using FluentMigrator;

using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(20231101145200, TransactionBehavior.None)]
public sealed class AddAccountingByStatusV1Type : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) =>
        """
        do $$
            begin
                if not exists (select 1 from pg_type where typname = 'accounting_by_status_v1') then
                    create type accounting_by_status_v1 as
                    (
                          id          bigint
                        , item_id     bigint
                        , created     integer
                        , delivered   integer
                        , canceled    integer
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
                drop type if exists accounting_by_status_v1;
            end
        $$;
        """;
}
