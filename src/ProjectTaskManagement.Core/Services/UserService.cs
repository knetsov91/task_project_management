using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Exceptions;
using ProjectTaskManagement.Core.Interfaces;

namespace ProjectTaskManagement.Core.Services;

public sealed class UserService(IUserRepository users, TimeProvider timeProvider)
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return users.GetByIdAsync(id, cancellationToken);
    }

    public async Task<User> CreateAsync(
        string name, string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var trimmedEmail = email.Trim();
        var normalizedEmail = trimmedEmail.ToUpperInvariant();

        if (await users.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new DuplicateEmailException();
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = trimmedEmail,
            NormalizedEmail = normalizedEmail,
            CreatedAt = timeProvider.GetUtcNow()
        };

        await users.AddAsync(user, cancellationToken);

        return user;
    }
}
