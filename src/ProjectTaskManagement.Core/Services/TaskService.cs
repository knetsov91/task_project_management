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
    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return tasks.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>?> GetByProjectIdAsync(
        Guid projectId, CancellationToken cancellationToken = default)
    {
        if (await projects.GetByIdAsync(projectId, cancellationToken) is null)
        {
            return null;
        }

        return await tasks.GetByProjectIdAsync(projectId, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>?> GetByAssigneeIdAsync(
        Guid assigneeId, CancellationToken cancellationToken = default)
    {
        if (await users.GetByIdAsync(assigneeId, cancellationToken) is null)
        {
            return null;
        }

        return await tasks.GetByAssigneeIdAsync(assigneeId, cancellationToken);
    }

    public async Task<TaskItem?> ChangeAssigneeAsync(
        Guid id, Guid? assignedToId, CancellationToken cancellationToken = default)
    {
        if (assignedToId == Guid.Empty)
        {
            throw new ArgumentException("An assignee ID cannot be empty.", nameof(assignedToId));
        }

        var task = await tasks.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            return null;
        }

        if (assignedToId is Guid assigneeId
            && await users.GetByIdAsync(assigneeId, cancellationToken) is null)
        {
            throw new UserNotFoundException(assigneeId);
        }

        task.AssignedToId = assignedToId;
        await tasks.UpdateAsync(task, cancellationToken);

        return task;
    }

    public async Task<TaskItem?> ChangeStatusAsync(
        Guid id, TaskItemStatus status, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Task status is invalid.");
        }

        var task = await tasks.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            return null;
        }

        var allowed = (task.Status, status) is
            (TaskItemStatus.Todo, TaskItemStatus.InProgress) or
            (TaskItemStatus.InProgress, TaskItemStatus.Todo) or
            (TaskItemStatus.InProgress, TaskItemStatus.Completed) or
            (TaskItemStatus.Completed, TaskItemStatus.InProgress);

        if (!allowed)
        {
            throw new InvalidTaskStatusTransitionException(task.Status, status);
        }

        task.Status = status;
        await tasks.UpdateAsync(task, cancellationToken);

        return task;
    }

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
