using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Core.Interfaces;

public interface IProjectRepository
{
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
}
