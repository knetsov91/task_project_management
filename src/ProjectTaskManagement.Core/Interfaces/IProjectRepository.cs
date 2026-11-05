using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Core.Interfaces;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetByOwnerIdAsync(
        Guid ownerId, CancellationToken cancellationToken = default);

    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
}
