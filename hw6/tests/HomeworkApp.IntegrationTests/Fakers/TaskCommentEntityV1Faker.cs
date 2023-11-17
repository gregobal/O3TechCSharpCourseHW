using AutoBogus;
using Bogus;
using HomeworkApp.Dal.Entities;
using HomeworkApp.IntegrationTests.Creators;

namespace HomeworkApp.IntegrationTests.Fakers;

public static class TaskCommentEntityV1Faker
{
    private static readonly object Lock = new();

    private static readonly Faker<TaskCommentEntityV1> Faker = new AutoFaker<TaskCommentEntityV1>()
        .RuleFor(x => x.Id, _ => Create.RandomId())
        .RuleFor(x => x.TaskId, _ => 1)
        .RuleFor(x => x.AuthorUserId, _ => Create.RandomId())
        .RuleFor(x => x.At, f => f.Date.RecentOffset().UtcDateTime)
        .RuleFor(x => x.ModifiedAt, _ => null)
        .RuleFor(x => x.DeletedAt, _ => null);

    public static TaskCommentEntityV1[] Generate(int count = 1)
    {
        lock (Lock)
        {
            return Faker.Generate(count).ToArray();
        }
    }

    public static TaskCommentEntityV1 WithId(
        this TaskCommentEntityV1 src,
        long id)
    {
        return src with { Id = id };
    }

    public static TaskCommentEntityV1 WithTaskId(
        this TaskCommentEntityV1 src,
        long taskId)
    {
        return src with { TaskId = taskId };
    }

    public static TaskCommentEntityV1 WithMessage(
        this TaskCommentEntityV1 src,
        string message)
    {
        return src with { Message = message };
    }

    public static TaskCommentEntityV1 WithSetDeleted(
        this TaskCommentEntityV1 src)
    {
        return src with { DeletedAt = DateTimeOffset.UtcNow };
    }
}