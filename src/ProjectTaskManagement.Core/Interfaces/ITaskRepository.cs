using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Core.Interfaces;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
}
