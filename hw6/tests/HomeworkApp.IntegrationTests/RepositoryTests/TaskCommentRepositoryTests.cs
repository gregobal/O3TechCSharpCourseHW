using FluentAssertions;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskCommentRepositoryTests
{
    private readonly ITaskCommentRepository _repository;

    public TaskCommentRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskCommentRepository;
    }

    [Fact]
    public async Task Add_TaskComment_Success()
    {
        // Arrange
        var taskComment = TaskCommentEntityV1Faker.Generate().First();

        // Act
        var result = await _repository.Add(taskComment, default);

        // Asserts
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_TaskComments_ByTaskId_Success()
    {
        // Arrange
        const int count = 10;
        const long taskId = 2;

        var taskComments = TaskCommentEntityV1Faker.Generate(count)
            .Select((tc, i) => i % 2 == 0 ? tc.WithTaskId(taskId) : tc)
            .ToList();
        List<long> taskCommentIds = new(count);

        foreach (var taskComment in taskComments)
        {
            var id = await _repository.Add(taskComment, default);
            taskCommentIds.Add(id);
        }

        var expected = taskComments.Select((tc, i) => tc.WithId(taskCommentIds[i]))
            .Where(tc => tc.TaskId == taskId)
            .OrderByDescending(tc => tc.At)
            .ToArray();

        // Act
        var results = await _repository.Get(new TaskCommentGetModel
        {
            TaskId = taskId,
            IncludeDeleted = true
        }, default);

        // Asserts
        results.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Get_TaskComments_ByTaskIdAndExcludeDeleted_Success()
    {
        // Arrange
        const int count = 10;
        const long taskId = 3;

        var taskComments = TaskCommentEntityV1Faker.Generate(count)
            .Select((tc, i) => i % 2 == 0 ? tc.WithTaskId(taskId).WithSetDeleted() : tc.WithTaskId(taskId))
            .ToList();
        List<long> taskCommentIds = new(count);

        foreach (var taskComment in taskComments)
        {
            var id = await _repository.Add(taskComment, default);
            taskCommentIds.Add(id);
        }

        var expected = taskComments.Select((tc, i) => tc.WithId(taskCommentIds[i]))
            .Where(tc => tc.DeletedAt is null)
            .OrderByDescending(tc => tc.At)
            .ToArray();

        // Act
        var results = await _repository.Get(new TaskCommentGetModel
        {
            TaskId = taskId,
            IncludeDeleted = false
        }, default);

        // Asserts
        results.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Update_TaskComment_Success()
    {
        // Arrange
        const long taskId = 4;

        var taskCommentForAdd = TaskCommentEntityV1Faker.Generate()
            .First()
            .WithTaskId(taskId);
        var id = await _repository.Add(taskCommentForAdd, default);
        var expected = taskCommentForAdd.WithId(id)
            .WithMessage("Updated from test");

        // Act
        var dateTimeOffsetBeforeAct = DateTimeOffset.UtcNow;
        await _repository.Update(expected, default);
        var dateTimeOffsetAfterAct = DateTimeOffset.UtcNow;

        // Asserts
        var results = await _repository.Get(new TaskCommentGetModel
        {
            TaskId = taskId,
            IncludeDeleted = true
        }, default);

        results.Should().HaveCount(1);

        var taskComment = results.Single();
        taskComment.Should().BeEquivalentTo(expected, opts =>
            opts.Excluding(x => x.ModifiedAt));
        taskComment.ModifiedAt.Should().BeAfter(dateTimeOffsetBeforeAct);
        taskComment.ModifiedAt.Should().BeBefore(dateTimeOffsetAfterAct);
    }

    [Fact]
    public async Task SetDeleted_TaskComment_Success()
    {
        // Arrange
        const long taskId = 5;

        var taskCommentForAdd = TaskCommentEntityV1Faker.Generate()
            .First()
            .WithTaskId(taskId);
        var id = await _repository.Add(taskCommentForAdd, default);

        // Act
        var dateTimeOffsetBeforeAct = DateTimeOffset.UtcNow;
        await _repository.SetDeleted(id, default);
        var dateTimeOffsetAfterAct = DateTimeOffset.UtcNow;

        // Asserts
        var results = await _repository.Get(new TaskCommentGetModel
        {
            TaskId = taskId,
            IncludeDeleted = true
        }, default);

        results.Should().HaveCount(1);

        var taskComment = results.Single();
        taskComment.DeletedAt.Should().NotBeNull();
        taskComment.DeletedAt.Should().BeAfter(dateTimeOffsetBeforeAct);
        taskComment.DeletedAt.Should().BeBefore(dateTimeOffsetAfterAct);
    }
}