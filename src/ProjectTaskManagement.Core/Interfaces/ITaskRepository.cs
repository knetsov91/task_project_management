using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Core.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(
        Guid projectId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItem>> GetByAssigneeIdAsync(
        Guid assigneeId, CancellationToken cancellationToken = default);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
}
