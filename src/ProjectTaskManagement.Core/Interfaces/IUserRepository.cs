using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Core.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(
        string normalizedEmail, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
