using Moq;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Exceptions;
using ProjectTaskManagement.Core.Interfaces;
using ProjectTaskManagement.Core.Services;

namespace ProjectTaskManagement.UnitTests.Services;

public class ProjectServiceTests
{
    [Fact]
    public async Task UpdateAsync_WhenOwnerExists_UpdatesProjectAndPreservesServerFields()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var projectId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);
        var project = new Project
        {
            Id = projectId,
            Name = "Original",
            Description = "Original description",
            OwnerId = Guid.NewGuid(),
            CreatedAt = createdAt
        };
        var newOwner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Alice",
            Email = "alice@example.com",
            NormalizedEmail = "ALICE@EXAMPLE.COM"
        };
        var projects = new Mock<IProjectRepository>(MockBehavior.Strict);
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var timeProvider = new Mock<TimeProvider>(MockBehavior.Strict);
        projects.Setup(repository => repository.GetByIdAsync(projectId, token)).ReturnsAsync(project);
        users.Setup(repository => repository.GetByIdAsync(newOwner.Id, token)).ReturnsAsync(newOwner);
        projects.Setup(repository => repository.UpdateAsync(project, token)).Returns(Task.CompletedTask);
        var service = new ProjectService(projects.Object, users.Object, timeProvider.Object);

        var result = await service.UpdateAsync(projectId, " Updated ", null, newOwner.Id, token);

        Assert.Same(project, result);
        Assert.Equal(projectId, project.Id);
        Assert.Equal(createdAt, project.CreatedAt);
        Assert.Equal("Updated", project.Name);
        Assert.Null(project.Description);
        Assert.Equal(newOwner.Id, project.OwnerId);
        projects.Verify(repository => repository.GetByIdAsync(projectId, token), Times.Once);
        users.Verify(repository => repository.GetByIdAsync(newOwner.Id, token), Times.Once);
        projects.Verify(repository => repository.UpdateAsync(project, token), Times.Once);
        projects.VerifyNoOtherCalls();
        users.VerifyNoOtherCalls();
        timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnerDoesNotExist_DoesNotChangeProject()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Original",
            Description = "Original description",
            OwnerId = Guid.NewGuid()
        };
        var originalOwnerId = project.OwnerId;
        var newOwnerId = Guid.NewGuid();
        var projects = new Mock<IProjectRepository>(MockBehavior.Strict);
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var timeProvider = new Mock<TimeProvider>(MockBehavior.Strict);
        projects.Setup(repository => repository.GetByIdAsync(project.Id, token))
            .ReturnsAsync(project);
        users.Setup(repository => repository.GetByIdAsync(newOwnerId, token))
            .ReturnsAsync((User?)null);
        var service = new ProjectService(projects.Object, users.Object, timeProvider.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(() => service.UpdateAsync(
            project.Id, "Changed", null, newOwnerId, token));

        Assert.Equal("Original", project.Name);
        Assert.Equal("Original description", project.Description);
        Assert.Equal(originalOwnerId, project.OwnerId);
        projects.Verify(repository => repository.GetByIdAsync(project.Id, token), Times.Once);
        users.Verify(repository => repository.GetByIdAsync(newOwnerId, token), Times.Once);
        projects.VerifyNoOtherCalls();
        users.VerifyNoOtherCalls();
        timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetByOwnerIdAsync_WhenOwnerDoesNotExist_ReturnsNull()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var ownerId = Guid.NewGuid();
        var projects = new Mock<IProjectRepository>(MockBehavior.Strict);
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var timeProvider = new Mock<TimeProvider>(MockBehavior.Strict);
        users.Setup(repository => repository.GetByIdAsync(ownerId, token))
            .ReturnsAsync((User?)null);
        var service = new ProjectService(projects.Object, users.Object, timeProvider.Object);

        var result = await service.GetByOwnerIdAsync(ownerId, token);

        Assert.Null(result);
        users.Verify(repository => repository.GetByIdAsync(ownerId, token), Times.Once);
        users.VerifyNoOtherCalls();
        projects.VerifyNoOtherCalls();
        timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_WhenOwnerExists_CreatesProjectSuccessfully()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var ownerId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);
        var owner = new User
        {
            Id = ownerId,
            Name = "Alice",
            Email = "alice@example.com",
            NormalizedEmail = "ALICE@EXAMPLE.COM"
        };
        var projects = new Mock<IProjectRepository>(MockBehavior.Strict);
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var timeProvider = new Mock<TimeProvider>(MockBehavior.Strict);
        users.Setup(repository => repository.GetByIdAsync(ownerId, token)).ReturnsAsync(owner);
        projects.Setup(repository => repository.AddAsync(It.IsAny<Project>(), token))
            .Returns(Task.CompletedTask);
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now);
        var service = new ProjectService(projects.Object, users.Object, timeProvider.Object);

        var project = await service.CreateAsync(
            " Portfolio API ", " Project management API ", ownerId, token);

        Assert.NotNull(project);
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal("Portfolio API", project.Name);
        Assert.Equal("Project management API", project.Description);
        Assert.Equal(ownerId, project.OwnerId);
        Assert.Equal(now, project.CreatedAt);
        Assert.Equal(TimeSpan.Zero, project.CreatedAt.Offset);
        users.Verify(repository => repository.GetByIdAsync(ownerId, token), Times.Once);
        projects.Verify(repository => repository.AddAsync(project, token), Times.Once);
        timeProvider.Verify(provider => provider.GetUtcNow(), Times.Once);
        users.VerifyNoOtherCalls();
        projects.VerifyNoOtherCalls();
        timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_WhenOwnerDoesNotExist_DoesNotCreateProject()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var ownerId = Guid.NewGuid();
        var projects = new Mock<IProjectRepository>(MockBehavior.Strict);
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var timeProvider = new Mock<TimeProvider>(MockBehavior.Strict);
        users.Setup(repository => repository.GetByIdAsync(ownerId, token))
            .ReturnsAsync((User?)null);
        var service = new ProjectService(projects.Object, users.Object, timeProvider.Object);

        var result = await service.CreateAsync("Portfolio API", null, ownerId, token);

        Assert.Null(result);
        users.Verify(repository => repository.GetByIdAsync(ownerId, token), Times.Once);
        users.VerifyNoOtherCalls();
        projects.VerifyNoOtherCalls();
        timeProvider.VerifyNoOtherCalls();
    }
}
