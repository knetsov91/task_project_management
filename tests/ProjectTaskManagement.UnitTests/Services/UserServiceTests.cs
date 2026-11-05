using Moq;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Exceptions;
using ProjectTaskManagement.Core.Interfaces;
using ProjectTaskManagement.Core.Services;

namespace ProjectTaskManagement.UnitTests.Services;

public class UserServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<IUserRepository> _users = new(MockBehavior.Strict);
    private readonly Mock<TimeProvider> _timeProvider = new(MockBehavior.Strict);

    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesNameAndEmail()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Alice",
            Email = "alice@example.com",
            NormalizedEmail = "ALICE@EXAMPLE.COM",
            CreatedAt = Now
        };
        _users.Setup(users => users.GetByIdAsync(userId, token)).ReturnsAsync(user);
        _users.Setup(users => users.EmailExistsAsync("ALICE.SMITH@EXAMPLE.COM", token))
            .ReturnsAsync(false);
        _users.Setup(users => users.UpdateAsync(user, token)).Returns(Task.CompletedTask);
        var service = new UserService(_users.Object, _timeProvider.Object);

        var result = await service.UpdateAsync(
            userId, " Alice Smith ", " alice.smith@example.com ", token);

        Assert.Same(user, result);
        Assert.Equal("Alice Smith", user.Name);
        Assert.Equal("alice.smith@example.com", user.Email);
        Assert.Equal("ALICE.SMITH@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal(Now, user.CreatedAt);
        _users.Verify(users => users.GetByIdAsync(userId, token), Times.Once);
        _users.Verify(users => users.EmailExistsAsync("ALICE.SMITH@EXAMPLE.COM", token), Times.Once);
        _users.Verify(users => users.UpdateAsync(user, token), Times.Once);
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUsersFromRepository()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        IReadOnlyList<User> expected =
        [
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Alice",
                Email = "alice@example.com",
                NormalizedEmail = "ALICE@EXAMPLE.COM"
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Bob",
                Email = "bob@example.com",
                NormalizedEmail = "BOB@EXAMPLE.COM"
            }
        ];
        _users.Setup(users => users.GetAllAsync(token)).ReturnsAsync(expected);
        var service = new UserService(_users.Object, _timeProvider.Object);

        var result = await service.GetAllAsync(token);

        Assert.Same(expected, result);
        _users.Verify(users => users.GetAllAsync(token), Times.Once);
        _users.VerifyNoOtherCalls();
        _timeProvider.VerifyNoOtherCalls();
    }

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
