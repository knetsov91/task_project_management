using Moq;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Interfaces;
using ProjectTaskManagement.Core.Services;

namespace ProjectTaskManagement.UnitTests.Services;

public class ProjectServiceTests
{
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
