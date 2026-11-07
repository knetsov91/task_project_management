using Moq;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Enums;
using ProjectTaskManagement.Core.Exceptions;
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
    public async Task GetByIdAsync_ReturnsTaskOrNullFromRepository(bool taskExists)
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var taskId = Guid.NewGuid();
        var expected = taskExists
            ? new TaskItem { Id = taskId, ProjectId = Guid.NewGuid(), Title = "Implement API" }
            : null;
        _tasks.Setup(repository => repository.GetByIdAsync(taskId, token)).ReturnsAsync(expected);
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        var result = await service.GetByIdAsync(taskId, token);

        Assert.Same(expected, result);
        _tasks.Verify(repository => repository.GetByIdAsync(taskId, token), Times.Once);
        _tasks.VerifyNoOtherCalls();
        _projects.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeAssigneeAsync_WhenTaskDoesNotExist_DoesNotSaveChanges()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var taskId = Guid.NewGuid();
        _tasks.Setup(repository => repository.GetByIdAsync(taskId, token)).ReturnsAsync((TaskItem?)null);
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        var result = await service.ChangeAssigneeAsync(taskId, Guid.NewGuid(), token);

        Assert.Null(result);
        _tasks.Verify(repository => repository.GetByIdAsync(taskId, token), Times.Once);
        _tasks.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _projects.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeAssigneeAsync_WhenUserDoesNotExist_PreservesCurrentAssignment()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var originalAssigneeId = Guid.NewGuid();
        var requestedAssigneeId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Title = "Implement API",
            AssignedToId = originalAssigneeId
        };
        _tasks.Setup(repository => repository.GetByIdAsync(task.Id, token)).ReturnsAsync(task);
        _users.Setup(repository => repository.GetByIdAsync(requestedAssigneeId, token))
            .ReturnsAsync((User?)null);
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => service.ChangeAssigneeAsync(task.Id, requestedAssigneeId, token));

        Assert.Equal(originalAssigneeId, task.AssignedToId);
        _tasks.Verify(repository => repository.GetByIdAsync(task.Id, token), Times.Once);
        _users.Verify(repository => repository.GetByIdAsync(requestedAssigneeId, token), Times.Once);
        _tasks.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _projects.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangeAssigneeAsync_WithValidAssignee_UpdatesAssignment(bool assignUser)
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Title = "Implement API",
            AssignedToId = Guid.NewGuid()
        };
        Guid? assigneeId = assignUser ? Guid.NewGuid() : null;
        _tasks.Setup(repository => repository.GetByIdAsync(task.Id, token)).ReturnsAsync(task);
        _tasks.Setup(repository => repository.UpdateAsync(task, token)).Returns(Task.CompletedTask);

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

        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        var result = await service.ChangeAssigneeAsync(task.Id, assigneeId, token);

        Assert.Same(task, result);
        Assert.Equal(assigneeId, task.AssignedToId);
        _tasks.Verify(repository => repository.GetByIdAsync(task.Id, token), Times.Once);
        _tasks.Verify(repository => repository.UpdateAsync(task, token), Times.Once);

        if (assigneeId is Guid expectedUserId)
        {
            _users.Verify(repository => repository.GetByIdAsync(expectedUserId, token), Times.Once);
        }

        _tasks.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _projects.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public async Task ChangeStatusAsync_WhenStatusIsUndefined_RejectsBeforeDatabaseAccess(int status)
    {
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ChangeStatusAsync(Guid.NewGuid(), (TaskItemStatus)status));

        _tasks.VerifyNoOtherCalls();
        _projects.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenTaskDoesNotExist_DoesNotSaveChanges()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var taskId = Guid.NewGuid();
        _tasks.Setup(repository => repository.GetByIdAsync(taskId, token)).ReturnsAsync((TaskItem?)null);
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        var result = await service.ChangeStatusAsync(taskId, TaskItemStatus.InProgress, token);

        Assert.Null(result);
        _tasks.Verify(repository => repository.GetByIdAsync(taskId, token), Times.Once);
        _tasks.VerifyNoOtherCalls();
        _projects.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(TaskItemStatus.Todo, TaskItemStatus.Completed)]
    [InlineData(TaskItemStatus.Completed, TaskItemStatus.Todo)]
    [InlineData(TaskItemStatus.Todo, TaskItemStatus.Todo)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.Completed, TaskItemStatus.Completed)]
    public async Task ChangeStatusAsync_WhenTransitionIsInvalid_DoesNotUpdateTask(
        TaskItemStatus currentStatus, TaskItemStatus nextStatus)
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Title = "Implement API",
            Status = currentStatus
        };
        _tasks.Setup(repository => repository.GetByIdAsync(task.Id, token)).ReturnsAsync(task);
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        await Assert.ThrowsAsync<InvalidTaskStatusTransitionException>(
            () => service.ChangeStatusAsync(task.Id, nextStatus, token));

        Assert.Equal(currentStatus, task.Status);
        _tasks.Verify(repository => repository.GetByIdAsync(task.Id, token), Times.Once);
        _tasks.VerifyNoOtherCalls();
        _projects.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(TaskItemStatus.Todo, TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.Todo)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.Completed)]
    [InlineData(TaskItemStatus.Completed, TaskItemStatus.InProgress)]
    public async Task ChangeStatusAsync_WhenTransitionIsAllowed_UpdatesTaskStatus(
        TaskItemStatus currentStatus, TaskItemStatus nextStatus)
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Title = "Implement API",
            Status = currentStatus
        };
        _tasks.Setup(repository => repository.GetByIdAsync(task.Id, token)).ReturnsAsync(task);
        _tasks.Setup(repository => repository.UpdateAsync(task, token)).Returns(Task.CompletedTask);
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        var result = await service.ChangeStatusAsync(task.Id, nextStatus, token);

        Assert.Same(task, result);
        Assert.Equal(nextStatus, task.Status);
        _tasks.Verify(repository => repository.GetByIdAsync(task.Id, token), Times.Once);
        _tasks.Verify(repository => repository.UpdateAsync(task, token), Times.Once);
        _tasks.VerifyNoOtherCalls();
        _projects.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_WhenAssigneeDoesNotExist_DoesNotCreateTask()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Portfolio API",
            OwnerId = Guid.NewGuid()
        };
        var assigneeId = Guid.NewGuid();
        _projects.Setup(repository => repository.GetByIdAsync(project.Id, token)).ReturnsAsync(project);
        _users.Setup(repository => repository.GetByIdAsync(assigneeId, token))
            .ReturnsAsync((User?)null);
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(() => service.CreateAsync(
            project.Id, "Implement API", null, TaskPriority.Medium, null, assigneeId, token));

        _projects.Verify(repository => repository.GetByIdAsync(project.Id, token), Times.Once);
        _users.Verify(repository => repository.GetByIdAsync(assigneeId, token), Times.Once);
        _projects.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _tasks.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_WhenProjectDoesNotExist_DoesNotCreateTask()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var projectId = Guid.NewGuid();
        _projects.Setup(repository => repository.GetByIdAsync(projectId, token))
            .ReturnsAsync((Project?)null);
        var service = new TaskService(_tasks.Object, _projects.Object, _users.Object, _timeProvider.Object);

        var result = await service.CreateAsync(
            projectId, "Implement API", null, TaskPriority.Medium, null, Guid.NewGuid(), token);

        Assert.Null(result);
        _projects.Verify(repository => repository.GetByIdAsync(projectId, token), Times.Once);
        _projects.VerifyNoOtherCalls();
        _tasks.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

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
