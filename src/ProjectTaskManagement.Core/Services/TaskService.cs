using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Enums;
using ProjectTaskManagement.Core.Exceptions;
using ProjectTaskManagement.Core.Interfaces;

namespace ProjectTaskManagement.Core.Services;

public class TaskService(
    ITaskRepository tasks,
    IProjectRepository projects,
    IUserRepository users,
    TimeProvider timeProvider)
{
    public async Task<TaskItem?> CreateAsync(
        Guid projectId,
        string title,
        string? description,
        TaskPriority priority,
        DateTimeOffset? dueDate,
        Guid? assignedToId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("A task project is required.", nameof(projectId));
        }

        if (assignedToId == Guid.Empty)
        {
            throw new ArgumentException("An assignee ID cannot be empty.", nameof(assignedToId));
        }

        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentOutOfRangeException(nameof(priority), "Task priority is invalid.");
        }

        if (await projects.GetByIdAsync(projectId, cancellationToken) is null)
        {
            return null;
        }

        if (assignedToId is Guid assigneeId
            && await users.GetByIdAsync(assigneeId, cancellationToken) is null)
        {
            throw new UserNotFoundException(assigneeId);
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            AssignedToId = assignedToId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = TaskItemStatus.Todo,
            Priority = priority,
            DueDate = dueDate?.ToUniversalTime(),
            CreatedAt = timeProvider.GetUtcNow()
        };

        await tasks.AddAsync(task, cancellationToken);

        return task;
    }
}
