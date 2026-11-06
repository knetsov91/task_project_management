using Moq;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Enums;
using ProjectTaskManagement.Core.Interfaces;
using ProjectTaskManagement.Core.Services;

namespace ProjectTaskManagement.UnitTests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _tasks = new(MockBehavior.Strict);
    private readonly Mock<IProjectRepository> _projects = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _users = new(MockBehavior.Strict);
    private readonly Mock<TimeProvider> _timeProvider = new(MockBehavior.Strict);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateAsync_WithExistingProject_CreatesTaskSuccessfully(bool assignUser)
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Portfolio API",
            OwnerId = Guid.NewGuid()
        };
        Guid? assigneeId = assignUser ? Guid.NewGuid() : null;
        var now = new DateTimeOffset(2026, 11, 6, 10, 0, 0, TimeSpan.Zero);
        var dueDate = new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.FromHours(2));
        _projects.Setup(repository => repository.GetByIdAsync(project.Id, token)).ReturnsAsync(project);

        if (assigneeId is Guid userId)
        {
            _users.Setup(repository => repository.GetByIdAsync(userId, token)).ReturnsAsync(new User
            {
                Id = userId,
                Name = "Alice",
                Email = "alice@example.com",
                NormalizedEmail = "ALICE@EXAMPLE.COM"
            });
        }

        _timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now);
        _tasks.Setup(repository => repository.AddAsync(It.IsAny<TaskItem>(), token))
            .Returns(Task.CompletedTask);
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        var task = await service.CreateAsync(
            project.Id, " Implement API ", " Create endpoints ", TaskPriority.High,
            dueDate, assigneeId, token);

        Assert.NotNull(task);
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(project.Id, task.ProjectId);
        Assert.Equal(assigneeId, task.AssignedToId);
        Assert.Equal("Implement API", task.Title);
        Assert.Equal("Create endpoints", task.Description);
        Assert.Equal(TaskItemStatus.Todo, task.Status);
        Assert.Equal(TaskPriority.High, task.Priority);
        Assert.Equal(dueDate.ToUniversalTime(), task.DueDate);
        Assert.Equal(TimeSpan.Zero, task.DueDate!.Value.Offset);
        Assert.Equal(now, task.CreatedAt);
        _projects.Verify(repository => repository.GetByIdAsync(project.Id, token), Times.Once);
        _tasks.Verify(repository => repository.AddAsync(task, token), Times.Once);
        _timeProvider.Verify(provider => provider.GetUtcNow(), Times.Once);

        if (assigneeId is Guid expectedUserId)
        {
            _users.Verify(repository => repository.GetByIdAsync(expectedUserId, token), Times.Once);
        }

        _projects.VerifyNoOtherCalls();
        _tasks.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }
}
