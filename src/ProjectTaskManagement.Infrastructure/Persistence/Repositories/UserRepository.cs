using Microsoft.EntityFrameworkCore;

namespace ProjectTaskManagement.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext dbContext)
{
    public Task<bool> EmailExistsAsync(
        string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(
            user => user.NormalizedEmail == normalizedEmail, cancellationToken);
    }
}
