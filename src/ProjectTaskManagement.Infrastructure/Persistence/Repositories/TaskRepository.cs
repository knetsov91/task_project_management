using Microsoft.EntityFrameworkCore;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Interfaces;

namespace ProjectTaskManagement.Infrastructure.Persistence.Repositories;

public class TaskRepository(AppDbContext dbContext) : ITaskRepository
{
    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Tasks.AsNoTracking()
            .SingleOrDefaultAsync(task => task.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(
        Guid projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tasks.AsNoTracking()
            .Where(task => task.ProjectId == projectId)
            .OrderBy(task => task.CreatedAt).ThenBy(task => task.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByAssigneeIdAsync(
        Guid assigneeId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tasks.AsNoTracking()
            .Where(task => task.AssignedToId == assigneeId)
            .OrderBy(task => task.CreatedAt).ThenBy(task => task.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        dbContext.Tasks.Update(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
