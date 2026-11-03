using Moq;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Exceptions;
using ProjectTaskManagement.Core.Interfaces;
using ProjectTaskManagement.Core.Services;

namespace ProjectTaskManagement.UnitTests.Services;

public sealed class UserServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<IUserRepository> _users = new(MockBehavior.Strict);
    private readonly Mock<TimeProvider> _timeProvider = new(MockBehavior.Strict);

    [Fact]
    public async Task CreateAsync_WhenEmailIsNotRegistered_CreatesUserSuccessfully()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        _users.Setup(users => users.EmailExistsAsync("ALICE@EXAMPLE.COM", token))
            .ReturnsAsync(false);
        _users.Setup(users => users.AddAsync(It.IsAny<User>(), token))
            .Returns(Task.CompletedTask);
        _timeProvider.Setup(time => time.GetUtcNow()).Returns(Now);
        var service = new UserService(_users.Object, _timeProvider.Object);

        var user = await service.CreateAsync("  Alice Smith  ", " Alice@Example.com ", token);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Alice Smith", user.Name);
        Assert.Equal("Alice@Example.com", user.Email);
        Assert.Equal("ALICE@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal(Now, user.CreatedAt);
        Assert.Equal(TimeSpan.Zero, user.CreatedAt.Offset);
        _users.Verify(users => users.EmailExistsAsync("ALICE@EXAMPLE.COM", token), Times.Once);
        _users.Verify(users => users.AddAsync(user, token), Times.Once);
        _users.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("ALICE@EXAMPLE.COM")]
    [InlineData(" Alice@Example.Com ")]
    public async Task CreateAsync_WhenEmailIsAlreadyRegistered_ThrowsDuplicateEmailException(string email)
    {
        _users.Setup(users => users.EmailExistsAsync("ALICE@EXAMPLE.COM", default))
            .ReturnsAsync(true);
        var service = new UserService(_users.Object, _timeProvider.Object);

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => service.CreateAsync("Alice", email));

        _users.Verify(users => users.EmailExistsAsync("ALICE@EXAMPLE.COM", default), Times.Once);
        _users.Verify(users => users.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

}
