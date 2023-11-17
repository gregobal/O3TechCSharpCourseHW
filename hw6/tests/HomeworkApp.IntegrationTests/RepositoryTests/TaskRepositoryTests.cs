using FluentAssertions;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Creators;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskRepositoryTests
{
    private readonly ITaskRepository _repository;

    public TaskRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskRepository;
    }

    [Fact]
    public async Task Add_Task_Success()
    {
        // Arrange
        const int count = 5;

        var tasks = TaskEntityV1Faker.Generate(count);
        
        // Act
        var results = await _repository.Add(tasks, default);

        // Asserts
        results.Should().HaveCount(count);
        results.Should().OnlyContain(x => x > 0);
    }
    
    [Fact]
    public async Task Get_SingleTask_Success()
    {
        // Arrange
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId);
        
        // Act
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);
        
        // Asserts
        results.Should().HaveCount(1);
        var task = results.Single();

        task.Should().BeEquivalentTo(expectedTask);
    }
    
    [Fact]
    public async Task AssignTask_Success()
    {
        // Arrange
        var assigneeUserId = Create.RandomId();
        
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId)
            .WithAssignedToUserId(assigneeUserId);
        var assign = AssignTaskModelFaker.Generate()
            .First()
            .WithTaskId(expectedTaskId)
            .WithAssignToUserId(assigneeUserId);
        
        // Act
        await _repository.Assign(assign, default);
        
        // Asserts
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);
        
        results.Should().HaveCount(1);
        var task = results.Single();
        
        expectedTask = expectedTask with {Status = assign.Status};
        task.Should().BeEquivalentTo(expectedTask);
    }

    [Fact]
    public async Task GetSubTasksInStatus_Success()
    {
        // Arrange
        var grandParentTask = TaskEntityV1Faker.Generate().First()
            .WithStatus(1)
            .WithParentTaskId(null);
        var grandParentTaskId = (await _repository.Add(new[] { grandParentTask }, default)).First();
        grandParentTask = grandParentTask.WithId(grandParentTaskId);

        var parentTask = TaskEntityV1Faker.Generate().First()
            .WithStatus(3)
            .WithParentTaskId(grandParentTask.Id);
        var parentTaskId = (await _repository.Add(new[] { parentTask }, default)).First();
        parentTask = parentTask.WithId(parentTaskId);

        var task1 = TaskEntityV1Faker.Generate().First()
            .WithStatus(2)
            .WithParentTaskId(parentTask.Id);
        var task2 = TaskEntityV1Faker.Generate().First()
            .WithStatus(2)
            .WithParentTaskId(parentTaskId);
        var taskIds = await _repository.Add(new[] { task1, task2 }, default);
        task1 = task1.WithId(taskIds[0]);
        task2 = task2.WithId(taskIds[1]);

        var subTask = TaskEntityV1Faker.Generate().First()
            .WithStatus(1)
            .WithParentTaskId(task2.Id);
        var subTaskId = (await _repository.Add(new[] { subTask }, default)).First();
        subTask = subTask.WithId(subTaskId);

        var subSubTask = TaskEntityV1Faker.Generate().First()
            .WithStatus(3)
            .WithParentTaskId(subTask.Id);
        var subSubTaskId = (await _repository.Add(new[] { subSubTask }, default)).First();
        subSubTask = subSubTask.WithId(subSubTaskId);

        var expectedForStatus3 = new[]
        {
            new SubTaskModel
            {
                TaskId = subSubTask.Id,
                Title = subSubTask.Title,
                Status = subSubTask.Status,
                ParentTaskIds = new[]
                {
                    subTask.Id,
                    task2.Id
                }
            }
        };

        var expectedForStatuses1And2 = new[]
        {
            new SubTaskModel
            {
                TaskId = task1.Id,
                Title = task1.Title,
                Status = task1.Status,
                ParentTaskIds = Array.Empty<long>()
            },
            new SubTaskModel
            {
                TaskId = task2.Id,
                Title = task2.Title,
                Status = task2.Status,
                ParentTaskIds = Array.Empty<long>()
            },
            new SubTaskModel
            {
                TaskId = subTask.Id,
                Title = subTask.Title,
                Status = subTask.Status,
                ParentTaskIds = new[]
                {
                    task2.Id
                }
            }
        };

        // Act
        var resultsForStatus3 = await _repository.GetSubTasksInStatus(parentTask.Id,
            new[] { 3 },
            default);

        var resultsForStatuses1And2 = await _repository.GetSubTasksInStatus(parentTask.Id,
            new[] { 1, 2 },
            default);

        // Asserts
        resultsForStatus3.Should().BeEquivalentTo(expectedForStatus3);

        resultsForStatuses1And2.OrderBy(x => x.TaskId)
            .Should().BeEquivalentTo(expectedForStatuses1And2.OrderBy(x => x.TaskId));
    }
}
