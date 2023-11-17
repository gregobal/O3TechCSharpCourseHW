using Dapper;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;

namespace HomeworkApp.Dal.Repositories;

public class TaskCommentRepository : PgRepository, ITaskCommentRepository
{
    public TaskCommentRepository(IOptions<DalOptions> dalSettings) : base(dalSettings.Value)
    {
    }

    public async Task<long> Add(TaskCommentEntityV1 model, CancellationToken token)
    {
        const string sqlQuery = @"
insert into task_comments (task_id, author_user_id, message, at, modified_at, deleted_at) 
values (@TaskId, @AuthorUserId, @Message, @At, @ModifiedAt, @DeletedAt)
returning id;
";

        await using var connection = await GetConnection();
        var id = await connection.QuerySingleAsync<long>(
            new CommandDefinition(
                sqlQuery,
                parameters: model,
                cancellationToken: token));

        return id;
    }

    public async Task Update(TaskCommentEntityV1 model, CancellationToken token)
    {
        const string sqlQuery = @"
update task_comments
   set message = @Message     
     , modified_at = @ModifiedAt     
 where id = @Id
";

        await using var connection = await GetConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                sqlQuery,
                parameters: new
                {
                    Id = model.Id,
                    Message = model.Message,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                cancellationToken: token));
    }

    public async Task SetDeleted(long taskCommentId, CancellationToken token)
    {
        const string sqlQuery = @"
update task_comments
   set deleted_at = @DeletedAt
 where id = @Id and deleted_at is null
";

        await using var connection = await GetConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                sqlQuery,
                parameters: new
                {
                    Id = taskCommentId,
                    DeletedAt = DateTimeOffset.UtcNow
                },
                cancellationToken: token));
    }

    public async Task<TaskCommentEntityV1[]> Get(TaskCommentGetModel model, CancellationToken token)
    {
        const string baseSql = @"
select id
     , task_id
     , author_user_id
     , message
     , at
     , modified_at
     , deleted_at    
  from task_comments
";

        var conditions = new List<string>
        {
            "task_id = @TaskId"
        };
        
        var @params = new DynamicParameters();
        @params.Add("TaskId", model.TaskId);

        if (!model.IncludeDeleted) conditions.Add("deleted_at is null");

        var cmd = new CommandDefinition(
            $"{baseSql} where {string.Join(" AND ", conditions)}",
            parameters: @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);

        await using var connection = await GetConnection();
        var result = (await connection.QueryAsync<TaskCommentEntityV1>(cmd))
            .OrderByDescending(tc => tc.At)
            .ToArray();

        return result;
    }
}