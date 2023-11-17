using FluentMigrator;

namespace Route256.Week5.Workshop.PriceCalculator.Dal.Migrations;

[Migration(20231025161600, TransactionBehavior.None)]
public class AlterTaskComments_AddModifiedAt : Migration {
    public override void Up()
    {
        const string sql = @"alter table task_comments add column modified_at timestamp with time zone null";
        
        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"alter table task_comments drop column if exists modified_at";

        Execute.Sql(sql);
    }
}